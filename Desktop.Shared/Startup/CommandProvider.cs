using System.CommandLine;
using CommunityToolkit.Diagnostics;
using Remotely.Desktop.Shared.Enums;

namespace Remotely.Desktop.Shared.Startup;
public static class CommandProvider
{
    /// <summary>
    /// Creates a <see cref="Command"/> for starting the remote control client.
    /// </summary>
    /// <param name="isRootCommand">Whether to create a <see cref="RootCommand"/> or <see cref="Command"/>.</param>
    /// <param name="commandLineDescription">The description for the command.</param>
    /// <param name="commandName">The name used to invoke the command.  Required if not a root command.</param>
    /// <returns></returns>
    public static Command CreateRemoteControlCommand(
        bool isRootCommand,
        string commandLineDescription,
        string commandName = "")
    {
        Command? rootCommand;

        if (isRootCommand)
        {
            rootCommand = new RootCommand(commandLineDescription);
        }
        else
        {
            Guard.IsNotNullOrWhiteSpace(commandName);
            rootCommand = new Command(commandName, commandLineDescription);
        }

        var hostOption = new Option<string>("--host", "The hostname of the server to which to connect (e.g. https://example.com).");
        hostOption.Aliases.Add("-h");
        rootCommand.Options.Add(hostOption);

        var modeOption = new Option<AppMode>(
            "--mode",
            description: "The remote control mode to use.  Either Attended, Unattended, or Chat.")
        {
            DefaultValueFactory = () => AppMode.Attended
        };
        modeOption.Aliases.Add("-m");
        rootCommand.Options.Add(modeOption);

        var pipeNameOption = new Option<string>("--pipe-name", "When AppMode is Chat, this is the pipe name used by the named pipes server.");
        pipeNameOption.Aliases.Add("-p");
        rootCommand.Options.Add(pipeNameOption);

        var sessionIdOption = new Option<string>("--session-id", 
            "In Unattended mode, this unique session ID will be assigned to this connection and " +
            "shared with the server.  The connection can then be found in the RemoteControlSessionCache " +
            "using this ID.");
        sessionIdOption.Aliases.Add("-s");
        rootCommand.Options.Add(sessionIdOption);

        var accessKeyOption = new Option<string>("--access-key", "In Unattended mode, secures access to the connection using the provided key.");
        accessKeyOption.Aliases.Add("-a");
        rootCommand.Options.Add(accessKeyOption);

        var requesterNameOption = new Option<string>("--requester-name", "The name of the technician requesting to connect.");
        requesterNameOption.Aliases.Add("-r");
        rootCommand.Options.Add(requesterNameOption);

        var organizationNameOption = new Option<string>("--org-name", "The organization name of the technician requesting to connect.");
        organizationNameOption.Aliases.Add("-o");
        rootCommand.Options.Add(organizationNameOption);

        var relaunchOption = new Option<bool>(
            "--relaunch",
            "Used to indicate that process is being relaunched from a previous session " +
            "and should notify viewers when it's ready.");
        rootCommand.Options.Add(relaunchOption);

        var viewersOption = new Option<string>(
            "--viewers",
            "Used with --relaunch.  Should be a comma-separated list of viewers' " +
            "SignalR connection IDs.");
        rootCommand.Options.Add(viewersOption);

        var elevateOption = new Option<bool>(
            "--elevate",
            "Must be called from a Windows service.  The process will relaunch " +
            "itself in the console session with elevated rights.");
        rootCommand.Options.Add(elevateOption);

        return rootCommand;
    }
}
