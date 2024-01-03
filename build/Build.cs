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
[GitHubActions
(
    "cd",
    GitHubActionsImage.Ubuntu2204,
    OnPushTags = ["*"],
    CacheIncludePatterns = [],
    CacheKeyFiles = [],
    FetchDepth = 0,
    InvokedTargets = [nameof(PushDockerImage), nameof(DockerLogin)],
    ImportSecrets = ["DOCKER_USER", "DOCKER_PASSWORD"]
)]
sealed class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Solution(GenerateProjects = true)] readonly Solution Solution = default!;
    [GitVersion] readonly GitVersion GitVersion = default!;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Runtime used for publishing. Default is dotnet default value")]
    readonly string? Runtime;

    [Parameter("Docker user for publishing")]
    readonly string DockerUser = default!;

    [Parameter("Docker password for publishing")]
    readonly string DockerPassword = default!;

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath ServerOutput => ArtifactsDirectory / "server";

    string DockerTag => $"huszky/transmission-extras:{GitVersion.NuGetVersionV2}";

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

    Target SetDockerLogger => _ => _
        .DependentFor(BuildDockerImage, PushDockerImage)
        .Executes(() => DockerLogger = (_, log) => Log.Information(log));

    Target BuildDockerImage => _ => _
        .DependsOn(Publish)
        .Executes(() =>
            DockerBuild(s => s
                .SetFile(Solution.TransmissionExtras_Server.Directory / "Dockerfile")
                .SetPath(RootDirectory)
                .SetTag($"huszky/transmission-extras:{GitVersion.NuGetVersionV2}")));

    Target DockerLogin => _ => _
        .Before(PushDockerImage)
        .Requires(() => DockerUser, () => DockerPassword)
        .Executes(() =>
            Docker($"login --username {DockerUser} --password {DockerPassword}"));

    Target PushDockerImage => _ => _
        .DependsOn(BuildDockerImage)
        .Executes(() =>
            DockerPush(s => s
                .SetName(DockerTag)));
}
