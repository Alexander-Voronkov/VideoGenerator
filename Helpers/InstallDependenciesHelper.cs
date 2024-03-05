using System.Diagnostics;
using VideoGenerator.Exceptions;

namespace VideoGenerator.Helpers;

public static class InstallDependenciesHelper
{
    /// <summary>
    /// Method for installing python via the chocolatey package manager
    /// </summary>
    /// <returns></returns>
    public static async Task InstallPython(CancellationToken token = default)
    {
        var chocoList = await GetChocoInstalledApps(token);

        if (!chocoList.Contains(
            "python",
            StringComparison.InvariantCultureIgnoreCase))
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd",
                Arguments = "/c choco install python -y",
                Verb = "runas",
                UseShellExecute = true,
            });

            await process.WaitForExitAsync(token);

            if (process.ExitCode != 0)
            {
                throw new InstallationFailedError($"An error occured while installing dependency: {nameof(InstallPython)}");
            }
        }
    }

    /// <summary>
    /// Method for installing choco package manager
    /// </summary>
    /// <returns></returns>
    public static async Task InstallChoco(CancellationToken token = default)
    {
        if (!Environment.GetEnvironmentVariable("PATH").Contains(
            "choco",
            StringComparison.InvariantCultureIgnoreCase))
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd",
                Verb = "runas",
                UseShellExecute = true,
                Arguments = "/c powershell.exe -NoProfile -ExecutionPolicy Bypass -Command \"iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))\" && SET PATH=%PATH%;%ALLUSERSPROFILE%\\chocolatey\\bin",
            });

            await process.WaitForExitAsync(token);

            if (process.ExitCode != 0)
            {
                throw new InstallationFailedError($"An error occured while installing dependency: {nameof(InstallChoco)}");
            }
        }
    }

    /// <summary>
    /// Method for installing whisper transcriber via pip
    /// </summary>
    /// <returns></returns>
    public static async Task InstallWhisper(CancellationToken token = default)
    {
        var chocoList = await GetChocoInstalledApps(token);

        if (!chocoList.Contains(
           "python",
           StringComparison.InvariantCultureIgnoreCase))
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = "/c pip install whisper",
                Verb = "runas",
                UseShellExecute = true,
            });

            await process.WaitForExitAsync(token);

            if (process.ExitCode != 0)
            {
                throw new InstallationFailedError($"An error occured while installing dependency: {nameof(InstallWhisper)}");
            }
        }
    }

    /// <summary>
    /// Method for installing ffmpeg via the chocolatey package manager
    /// </summary>
    /// <returns></returns>
    public static async Task InstallFFmpeg(CancellationToken token = default)
    {
        var chocoList = await GetChocoInstalledApps(token);

        if (!chocoList.Contains(
            "ffmpeg",
            StringComparison.InvariantCultureIgnoreCase))
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd",
                Arguments = "/c choco install ffmpeg -y",
                Verb = "runas",
                UseShellExecute = true,
                CreateNoWindow = true,
            });

            await process.WaitForExitAsync(token);

            if (process.ExitCode != 0)
            {
                throw new InstallationFailedError($"An error occured while installing dependency: {nameof(InstallFFmpeg)}");
            }
        }
    }

    /// <summary>
    /// Method for installing all dependencies
    /// </summary>
    /// <returns></returns>
    public static async Task InstallAllDependencies(CancellationToken token = default)
    {
        await InstallChoco(token);
        await Task.WhenAll(InstallFFmpeg(token), InstallPython(token));
        await InstallWhisper(token);
    }

    /// <summary>
    /// Method for getting the list of applications installed by choco
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<string> GetChocoInstalledApps(CancellationToken token = default)
    {
        var process = Process.Start(new ProcessStartInfo()
        {
            FileName = "cmd",
            Arguments = "/c choco --list",
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        });
        process.Start();
        await process.WaitForExitAsync(token);
        var processOutput = await process.StandardOutput.ReadToEndAsync(token);
        return processOutput;
    }
}