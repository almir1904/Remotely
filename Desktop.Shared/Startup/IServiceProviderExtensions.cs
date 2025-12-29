using Remotely.Desktop.Shared.Abstractions;
using Remotely.Desktop.Shared.Enums;
using Remotely.Desktop.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remotely.Shared.Primitives;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Runtime.Versioning;
using Remotely.Desktop.Native.Windows;

namespace Remotely.Desktop.Shared.Startup;

public static class IServiceProviderExtensions
{
    /// <summary>
    /// Runs the remote control startup with the specified arguments.
    /// </summary>
    public static async Task<Result> UseRemoteControlClient(
        this IServiceProvider services,
        string host,
        AppMode mode,
        string pipeName,
        string sessionId,
        string accessKey,
        string requesterName,
        string organizationName,
        bool relaunch,
        string viewers,
        bool elevate)
    {
        try
        {
            var logger = services.GetRequiredService<ILogger<IServiceProvider>>();
            TaskScheduler.UnobservedTaskException += (object? sender, UnobservedTaskExceptionEventArgs e) =>
            {
                HandleUnobservedTask(e, logger);
            };

            if (OperatingSystem.IsWindows() && elevate)
            {
                RelaunchElevated();
                return Result.Ok();
            }

            var appState = services.GetRequiredService<IAppState>();
            appState.Configure(
                host ?? string.Empty,
                mode,
                sessionId ?? string.Empty,
                accessKey ?? string.Empty,
                requesterName ?? string.Empty,
                organizationName ?? string.Empty,
                pipeName ?? string.Empty,
                relaunch,
                viewers ?? string.Empty,
                elevate);

            StaticServiceProvider.Instance = services;

            var appStartup = services.GetRequiredService<IAppStartup>();
            await appStartup.Run();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex);
        }
       
    }

    /// <summary>
    /// Runs the remote control startup as a root command.  This uses the System.CommandLine package.
    /// </summary>
    /// <param name="services">The service provider fo rthe app using this library.</param>
    /// <param name="args">The original command line arguments passed into the app.</param>
    /// <param name="commandLineDescription">The description to use for the remote control command.</param>
    /// <param name="serverUri">If provided, will be used as a fallback if --host option is missing.</param>
    /// <returns></returns>
    public static async Task<Result> UseRemoteControlClient(
        this IServiceProvider services,
        string[] args,
        string commandLineDescription,
        string serverUri = "",
        bool treatUnmatchedArgsAsErrors = true)
    {
        try
        {
            var rootCommand = CommandProvider.CreateRemoteControlCommand(true, commandLineDescription);

            // Get options from the command for SetHandler binding
            var hostOption = rootCommand.Options.OfType<Option<string>>().FirstOrDefault(o => o.Aliases.Contains("--host")) 
                ?? throw new InvalidOperationException("Host option not found");
            var modeOption = rootCommand.Options.OfType<Option<AppMode>>().FirstOrDefault(o => o.Aliases.Contains("--mode")) 
                ?? throw new InvalidOperationException("Mode option not found");
            var pipeNameOption = rootCommand.Options.OfType<Option<string>>().FirstOrDefault(o => o.Aliases.Contains("--pipe-name")) 
                ?? throw new InvalidOperationException("Pipe name option not found");
            var sessionIdOption = rootCommand.Options.OfType<Option<string>>().FirstOrDefault(o => o.Aliases.Contains("--session-id")) 
                ?? throw new InvalidOperationException("Session ID option not found");
            var accessKeyOption = rootCommand.Options.OfType<Option<string>>().FirstOrDefault(o => o.Aliases.Contains("--access-key")) 
                ?? throw new InvalidOperationException("Access key option not found");
            var requesterNameOption = rootCommand.Options.OfType<Option<string>>().FirstOrDefault(o => o.Aliases.Contains("--requester-name")) 
                ?? throw new InvalidOperationException("Requester name option not found");
            var organizationNameOption = rootCommand.Options.OfType<Option<string>>().FirstOrDefault(o => o.Aliases.Contains("--org-name")) 
                ?? throw new InvalidOperationException("Organization name option not found");
            var relaunchOption = rootCommand.Options.OfType<Option<bool>>().FirstOrDefault(o => o.Aliases.Contains("--relaunch")) 
                ?? throw new InvalidOperationException("Relaunch option not found");
            var viewersOption = rootCommand.Options.OfType<Option<string>>().FirstOrDefault(o => o.Aliases.Contains("--viewers")) 
                ?? throw new InvalidOperationException("Viewers option not found");
            var elevateOption = rootCommand.Options.OfType<Option<bool>>().FirstOrDefault(o => o.Aliases.Contains("--elevate")) 
                ?? throw new InvalidOperationException("Elevate option not found");

            // Create a handler that processes the command
            var handler = new RemoteControlCommandHandler(
                services,
                serverUri,
                hostOption,
                modeOption,
                pipeNameOption,
                sessionIdOption,
                accessKeyOption,
                requesterNameOption,
                organizationNameOption,
                relaunchOption,
                viewersOption,
                elevateOption);
            rootCommand.Handler = handler;

            rootCommand.TreatUnmatchedTokensAsErrors = treatUnmatchedArgsAsErrors;

            // Parse and invoke the command
            var parseResult = rootCommand.Parse(args);
            var result = await parseResult.InvokeAsync();

            if (result == 0)
            {
                return Result.Ok();
            }
            return Result.Fail($"Remote control command returned code {result}.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex);
        }
    }

    // This shouldn't be required in modern .NET to prevent the app from crashing,
    // but it could be useful to log it.
    private static void HandleUnobservedTask(
        UnobservedTaskExceptionEventArgs e, 
        ILogger<IServiceProvider> logger)
    {
        e.SetObserved();
        logger.LogError(e.Exception, "An unobserved task exception occurred.");
    }

    [SupportedOSPlatform("windows")]
    private static void RelaunchElevated()
    {
        var commandLine = Win32Interop.GetCommandLine().Replace(" --elevate", "");

        Console.WriteLine($"Elevating process {commandLine}.");
        var result = Win32Interop.CreateInteractiveSystemProcess(
            commandLine,
            -1,
            false,
            out var procInfo);
        Console.WriteLine($"Elevate result: {result}. Process ID: {procInfo.dwProcessId}.");
        Environment.Exit(0);
    }

    private class RemoteControlCommandHandler : ICommandHandler
    {
        private readonly IServiceProvider _services;
        private readonly string _serverUri;
        private readonly Option<string> _hostOption;
        private readonly Option<AppMode> _modeOption;
        private readonly Option<string> _pipeNameOption;
        private readonly Option<string> _sessionIdOption;
        private readonly Option<string> _accessKeyOption;
        private readonly Option<string> _requesterNameOption;
        private readonly Option<string> _organizationNameOption;
        private readonly Option<bool> _relaunchOption;
        private readonly Option<string> _viewersOption;
        private readonly Option<bool> _elevateOption;

        public RemoteControlCommandHandler(
            IServiceProvider services,
            string serverUri,
            Option<string> hostOption,
            Option<AppMode> modeOption,
            Option<string> pipeNameOption,
            Option<string> sessionIdOption,
            Option<string> accessKeyOption,
            Option<string> requesterNameOption,
            Option<string> organizationNameOption,
            Option<bool> relaunchOption,
            Option<string> viewersOption,
            Option<bool> elevateOption)
        {
            _services = services;
            _serverUri = serverUri;
            _hostOption = hostOption;
            _modeOption = modeOption;
            _pipeNameOption = pipeNameOption;
            _sessionIdOption = sessionIdOption;
            _accessKeyOption = accessKeyOption;
            _requesterNameOption = requesterNameOption;
            _organizationNameOption = organizationNameOption;
            _relaunchOption = relaunchOption;
            _viewersOption = viewersOption;
            _elevateOption = elevateOption;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var host = context.ParseResult.GetValueForOption(_hostOption) ?? string.Empty;
            var mode = context.ParseResult.GetValueForOption(_modeOption);
            var pipeName = context.ParseResult.GetValueForOption(_pipeNameOption) ?? string.Empty;
            var sessionId = context.ParseResult.GetValueForOption(_sessionIdOption) ?? string.Empty;
            var accessKey = context.ParseResult.GetValueForOption(_accessKeyOption) ?? string.Empty;
            var requesterName = context.ParseResult.GetValueForOption(_requesterNameOption) ?? string.Empty;
            var organizationName = context.ParseResult.GetValueForOption(_organizationNameOption) ?? string.Empty;
            var relaunch = context.ParseResult.GetValueForOption(_relaunchOption);
            var viewers = context.ParseResult.GetValueForOption(_viewersOption) ?? string.Empty;
            var elevate = context.ParseResult.GetValueForOption(_elevateOption);

            // Validate pipe name when mode is Chat
            if (mode == AppMode.Chat && string.IsNullOrWhiteSpace(pipeName))
            {
                Console.Error.WriteLine("A pipe name must be specified when AppMode is Chat.");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(_serverUri))
            {
                host = _serverUri;
            }

            var result = await _services.UseRemoteControlClient(
                host,
                mode,
                pipeName,
                sessionId,
                accessKey,
                requesterName,
                organizationName,
                relaunch,
                viewers,
                elevate);

            return result.IsFailure ? 1 : 0;
        }
    }
}
