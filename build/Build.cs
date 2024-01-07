using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;

using Serilog;

using static Nuke.Common.Tools.Docker.DockerTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[GitHubActions
(
    "ci",
    GitHubActionsImage.Ubuntu2204,
    OnPushBranches = ["main", "develop"],
    OnPullRequestBranches = ["main", "develop"],
    CacheIncludePatterns = [],
    CacheKeyFiles = [],
    FetchDepth = 0,
    InvokedTargets = [nameof(BuildDockerImage)]
)]
sealed class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    private readonly List<string> CreatedImages = [];

    [Solution(GenerateProjects = true)] readonly Solution Solution = default!;
    [GitVersion] readonly GitVersion GitVersion = default!;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Runtime used for publishing. Default is dotnet default value")]
    readonly string? Runtime;

    [Parameter("Env values for docker run. Sparate value decalrations by ';'")]
    readonly string? RunEnvs;

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath ServerOutput => ArtifactsDirectory / "server";

    GitHubActions GitHubActions => GitHubActions.Instance;

    string DockerName => "huszky/transmission-extras";
    string DockerTag => $"{DockerName}:{GitVersion.NuGetVersionV2}";
    string DockerLatestTag => $"{DockerName}:latest";

    string DebugContainerName => "transmission-extras-debug";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            ArtifactsDirectory.CreateOrCleanDirectory();

            DotNetClean(s => s
                .SetProject(Solution));
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
            DotNetRestore(s => s
                .SetProjectFile(Solution)));

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(GitVersion.NuGetVersionV2)
                .EnableNoRestore()));

    Target Publish => _ => _
        .DependsOn(Clean)
        .Executes(() =>
            DotNetPublish(s =>
            {
                s = s
                    .SetProject(Solution.TransmissionExtras_Server)
                    .SetOutput(ServerOutput)
                    .SetConfiguration(Configuration)
                    .SetVersion(GitVersion.NuGetVersionV2);

                if (Runtime is not null)
                {
                    s = s.SetRuntime(Runtime);
                }

                return s;
            }));

    Target RemovePublishFiles => _ => _
        .After(Publish)
        .TriggeredBy(Publish)
        .Before(BuildDockerImage)
        .Executes(() =>
        {
            (ServerOutput / "jobs.json").DeleteFile();
            (ServerOutput / "appsettings.Development.json").DeleteFile();
        });

    Target SetDockerLogger => _ => _
        .DependentFor(BuildDockerImage, PushDockerImage)
        .Executes(() => DockerLogger = (_, log) => Log.Information(log));

    Target BuildDockerImage => _ => _
        .DependsOn(Publish)
        .Executes(() =>
            DockerBuild(s =>
            {
                string[] tags = GitHubActions?.EventName != "workflow_dispatch"
                    ? [DockerTag, DockerLatestTag]
                    : [DockerTag];

                CreatedImages.AddRange(tags);

                return s
                    .SetFile(Solution.TransmissionExtras_Server.Directory / "Dockerfile")
                    .SetPath(RootDirectory)
                    .SetTag(tags);
            }));

    Target RunDocker => _ => _
        .After(BuildDockerImage)
        .Executes(() =>
            DockerRun(s => s
                .SetName(DebugContainerName)
                .SetImage(DockerTag)
                .SetEnv(RunEnvs?.Split(';') ?? [])
                .SetVolume($"{Solution.TransmissionExtras_Server.Directory / "jobs.json"}:/app/jobs.json")));

    Target RemoveDockerContainer => _ => _
        .TriggeredBy(RunDocker)
        .AssuredAfterFailure()
        .Executes(() =>
        {
            DockerStop(s => s.SetContainers(DebugContainerName));
            DockerRm(s => s.SetContainers(DebugContainerName));
        });

    Target PushDockerImage => _ => _
        .DependsOn(BuildDockerImage)
        .Executes(() =>
            CreatedImages.ForEach(i =>
                DockerPush(s => s
                    .SetName(i))));
}
