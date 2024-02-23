using System.Numerics;

namespace ImGuiNET;

public class ImGuiConsole {
    private string m_ConsoleName;
    private bool m_Open = true;
    private List<string> m_Items = new List<string>();
    private bool m_AutoScroll = true;
    private bool m_ScrollToBottom;
    private float m_WindowAlpha = 1.0f;
    private string m_Buffer = string.Empty;
    private string m_Filter = string.Empty;
    private bool m_ColoredOutput = true;
    private bool m_FilterBar = true;
    private bool m_TimeStamps = true;
    private Dictionary<string, Action<string>> commandRegistry;

    public static Vector4 color_command = new Vector4(1.0f, 1.0f, 1.0f, 1.0f); // COL_COMMAND
    public static Vector4 color_log = new Vector4(1.0f, 1.0f, 1.0f, 0.5f); // COL_LOG
    public static Vector4 color_warning = new Vector4(1.0f, 0.87f, 0.37f, 1.0f); // COL_WARNING
    public static Vector4 color_error = new Vector4(1.0f, 0.365f, 0.365f, 1.0f); // COL_ERROR
    public static Vector4 color_info = new Vector4(0.46f, 0.96f, 0.46f, 1.0f); // COL_INFO
    public static Vector4 color_timestamp = new Vector4(1.0f, 1.0f, 1.0f, 0.5f); // COL_TIMESTAMP

    public ImGuiConsole(string name) {
        m_ConsoleName = name;
        commandRegistry = new Dictionary<string, Action<string>>();
        RegisterConsoleCommands();
    }

    private void RegisterConsoleCommands() {
        // Register a 'clear' command to clear the console log
        commandRegistry["clear"] = (args) => m_Items.Clear();

        // Register a 'filter' command to apply a filter to the log output
        commandRegistry["filter"] = (args) => m_Filter = args;
    }

    public void ExecuteCommand(string input) {
        // Split the input into command and arguments
        var parts = input.Split(new[] { ' ' }, 2);
        var command = parts[0];
        var args = parts.Length > 1 ? parts[1] : string.Empty;

        if (commandRegistry.TryGetValue(command, out var action)) {
            action(args); // Execute the command with arguments
        } else {
            AddLog($"Unknown command: {command}");
        }
    }

    public void AddLog(string message) {
        // Assuming m_Items is a List<string> that stores your console log messages
        m_Items.Add(message);

        // Optionally, you might want to automatically scroll to the bottom when a new message is added
        m_ScrollToBottom = true;
    }

    public void Draw() {
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, m_WindowAlpha);
        if (ImGui.Begin(m_ConsoleName, ref m_Open, ImGuiWindowFlags.MenuBar)) {

            MenuBar();

            // Call the FilterBar method if implemented, else keep inline code
            FilterBar(); // Assuming FilterBar() is implemented

            // Log window encapsulated in a method
            LogWindow(); // Make sure to implement this as per previous instructions

            // Input bar encapsulated in a method
            InputBar(); // Adjust according to the InputBar() method described earlier

            ImGui.End();
        }

        ImGui.PopStyleVar();
    }

    private void MenuBar() {
        if (ImGui.BeginMenuBar()) {
            if (ImGui.BeginMenu("File")) {
                // Add items to the File menu
                if (ImGui.MenuItem("Exit", "Alt+F4")) {
                    m_Open = false; // Sets a flag to close the console window
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit")) {
                // Add items to the Edit menu
                if (ImGui.MenuItem("Clear", "CTRL+C")) {
                    ClearLog(); // Clears the console log
                }
                ImGui.EndMenu();
            }

            // Additional menus can be added here
            // Example: Help menu
            if (ImGui.BeginMenu("Help")) {
                if (ImGui.MenuItem("About")) {
                    // Display about information or set a flag to show an about window
                }
                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }


    public void InputBar() {
        // Reserve space for the input bar at the bottom
        float footerHeight = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + footerHeight);

        // Input text flags for the input bar
        ImGuiInputTextFlags inputFlags = ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackCompletion;

        bool reclaimFocus = false;
        // Render the input text field
        if (ImGui.InputText("##Input", ref m_Buffer, (uint)m_Buffer.Length, inputFlags)) {
            if (!string.IsNullOrEmpty(m_Buffer)) {
                ExecuteCommand(m_Buffer); // Assuming ExecuteCommand is implemented to handle command logic
            }

            reclaimFocus = true;
        }

        // Auto-focus on window apparition
        ImGui.SetItemDefaultFocus();
        if (reclaimFocus)
            ImGui.SetKeyboardFocusHere(-1); // Set focus back to the input field
    }

    private void DefaultSettings() {
        // Initialize or reset settings to their default values
        m_AutoScroll = true;
        m_ScrollToBottom = false;
        m_ColoredOutput = true;
        m_FilterBar = true;
        m_TimeStamps = true;
        m_WindowAlpha = 1.0f;
    }

    public void FilterBar() {
        if (ImGui.InputText("##Filter", ref m_Filter, (uint)m_Filter.Length, ImGuiInputTextFlags.EnterReturnsTrue)) {
        }
    }

    public void LogWindow() {
        // Reserve enough space for the filter bar plus one line of text, ensuring there's room for input
        float footerHeight = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
        // Begin a child region with a calculated height to leave space for the footer. Enable a vertical scrollbar.
        ImGui.BeginChild("Log", new Vector2(0, -footerHeight), ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);

        // Optional: Push a monospaced font here if you have one for better log readability
        // ImGui.PushFont(yourMonospacedFont);

        foreach (string item in m_Items)
        {
            // If there's a filter set, skip messages that don't contain the filter string
            if (!string.IsNullOrEmpty(m_Filter) && !item.Contains(m_Filter, StringComparison.OrdinalIgnoreCase))
                continue;

            // Determine message color based on content, or use default color
            Vector4 color = Vector4.One; // White as default color
            if (item.Contains("[Error]")) color = color_error; // Red for errors
            else if (item.Contains("[Warning]")) color = color_warning;

            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextUnformatted(item);
            ImGui.PopStyleColor();
        }

        // Optional: Pop the monospaced font here if you pushed one

        if (m_ScrollToBottom || (m_AutoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY()))
            ImGui.SetScrollHereY(1.0f);

        m_ScrollToBottom = false;

        ImGui.EndChild();
    }

    private void ClearLog() {
        m_Items.Clear(); // Clears all items from the log

        // Optional: Add a log message indicating the console was cleared, if desired
        AddLog("Console cleared.");

        // Ensure the scroll position is reset if needed
        m_ScrollToBottom = true;
    }

    public static bool InputText(string label, ref string str, ImGuiInputTextFlags flags = 0) {
        if (ImGui.InputText(label, ref str, (uint)str.Length, flags)) {
            return true;
        }

        return false;
    }
}