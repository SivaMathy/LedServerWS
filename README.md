# WebSocket Server Application

This application demonstrates a WebSocket server implementation using C# with the WebSocketSharp library. It provides two WebSocket services: one for controlling LEDs and another for handling user logins.

## Prerequisites

- .NET Framework installed on your system.
- WebSocketSharp library installed via NuGet package manager.

## Running the Application

1. Clone or download the repository to your local machine.
2. Open the solution in Visual Studio or any compatible C# IDE.
3. Build the solution to ensure all dependencies are resolved.
4. Run the `ConsoleApp1` project.

### LED Control WebSocket Service

Connect to `ws://localhost:7890/ledcontrol` to control LEDs. Use the following message formats:

- To toggle an LED status:
  - Format: `status:LED_ID:USER_ROLE`
  - Example: `status:2:Admin`
- To get initial LED states:
  - Send: `getInitialStates`

### Login WebSocket Service

Connect to `ws://localhost:7890/login` for user login. Use the following message formats:

- To log in:
  - Format: `login:EMAIL,PASSWORD`
  - Example: `login:user@gmail.com,user`
