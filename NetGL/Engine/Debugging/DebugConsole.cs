namespace NetGL;

using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public static class DebugConsole {
    private static readonly List<Message> messages = new();
    private static readonly List<string> history = new();
    //private static DebugListener? listener;
    private static readonly Dictionary<string, Action<string[]>> commands = new();
    private static bool m_TimeStamps = true;
    private static bool m_ColoredOutput = true;
    private static bool m_ScrollToBottom;
    private static bool m_AutoScroll = true;
    private static bool m_WasPrevFrameTabCompletion;

    public static string text_filter = string.Empty;
    public static bool show_toolbar = false;
    public static bool show_input = false;

    private static string m_Buffer = "";
    private static readonly uint m_Buffer_size = 256;
    private static readonly List<string> m_CmdSuggestions = new();
    private static int m_HistoryIndex;
    private static readonly Dictionary<string, Action<string[]>> cmdAutocomplete = new();
    private static readonly Dictionary<string, string> varAutocomplete = new();
    private static bool m_resetModal;
    private static float m_WindowAlpha = 1;
    private static readonly ColorPalette consoleColorPalette = new();
    private static readonly string m_ConsoleName = "Console";
    private static bool m_consoleOpen;
    private static DebugListener listener;
    private const int max_messages = 1024;

    public static void initialize(bool listen_to_console = true) {
        default_settings();

        //if(listen_to_console) listener = new DebugListener();

        register_command("clear", static _ => messages.Clear());
    }

    private static void default_settings() {
        m_WindowAlpha = 1f;
        consoleColorPalette[Severity.Command] = new(1.0f, 1.0f, 1.0f, 1.0f);
        consoleColorPalette[Severity.Log] = new(1.0f, 1.0f, 1.0f, 0.5f);
        consoleColorPalette[Severity.Warning] = new(1.0f, 0.87f, 0.37f, 1.0f);
        consoleColorPalette[Severity.Error] = new(1.0f, 0.365f, 0.365f, 1.0f);
        consoleColorPalette[Severity.Info] = new(0.46f, 0.96f, 0.46f, 1.0f);
        consoleColorPalette[Severity.Timestamp] = new(1.0f, 1.0f, 1.0f, 0.5f);
    }

    public static void register_command(string command, Action<string[]> callback) {
        commands.Add(command, callback);
        cmdAutocomplete.Add(command, callback);
    }

    public static void log(Severity type, string msg) {
        messages.Add(new Message(type, msg));
        if (messages.Count > max_messages)
            messages.Remove(messages[0]);

        m_ScrollToBottom = true;
    }

    public static void log(Exception? e) {
        messages.Add(new Message(Severity.Error, e?.ToString() ?? string.Empty));
        if (messages.Count > max_messages)
            messages.Remove(messages[0]);

        m_ScrollToBottom = true;
    }

    public static void log(string source, string msg) {
        var type = Severity.Log;

        if (msg.Contains("error", StringComparison.CurrentCultureIgnoreCase))
            type = Severity.Error;

        if (msg.Contains("warn", StringComparison.CurrentCultureIgnoreCase))
            type = Severity.Warning;

        if (msg.Contains("warning", StringComparison.CurrentCultureIgnoreCase))
            type = Severity.Warning;

        messages.Add(new Message(type, source, msg));
        if (messages.Count > max_messages)
            messages.Remove(messages[0]);

        m_ScrollToBottom = true;
    }

    public static void log(string msg) {
        Severity type = Severity.Log;

        if (msg.Contains("error", StringComparison.CurrentCultureIgnoreCase))
            type = Severity.Error;

        if (msg.Contains("warn", StringComparison.CurrentCultureIgnoreCase))
            type = Severity.Warning;

        if (msg.Contains("warning", StringComparison.CurrentCultureIgnoreCase))
            type = Severity.Warning;

        messages.Add(new Message(type, msg));
        if (messages.Count > max_messages)
            messages.Remove(messages[0]);

        m_ScrollToBottom = true;
    }

    public static void draw() {
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, m_WindowAlpha);

        // Calculate position and size for docking to the bottom
        Vector2 viewportSize = ImGui.GetIO().DisplaySize / ImGui.GetIO().DisplayFramebufferScale;
        float consoleHeight = 200;
        Vector2 consolePos = new Vector2(0, viewportSize.Y - consoleHeight);
        Vector2 consoleSize = new Vector2(viewportSize.X * 0.82f, consoleHeight);

        ImGui.SetNextWindowPos(consolePos);
        ImGui.SetNextWindowSize(consoleSize);

        if (!ImGui.Begin(m_ConsoleName, ref m_consoleOpen, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove)) {
            ImGui.PopStyleVar(1);
            ImGui.End();
            return;
        }

        ImGui.PopStyleVar();

        if(show_toolbar)
            tool_bar();

        log_window();

        if(show_input)
            input_bar();

        ImGui.End();
    }

    private static void log_window() {
        float footerHeightToReserve = show_toolbar ? ImGui.GetStyle().ItemSpacing.Y : 0;

        if (ImGui.BeginChild("ScrollRegion##", new Vector2(0, show_input ? -ImGui.GetFrameHeightWithSpacing() : 0 -footerHeightToReserve))) {
            // Display colored command output.
            float timestamp_width = ImGui.CalcTextSize("00:00:00:0000").X;
            int count = 0;

            // Wrap items.

            // Display items.
            for (int i = 0; i < messages.Count; i++) {
                var item = messages[i];

                // Exit if word is filtered.
                if (text_filter.Length != 0 && !item.message.Contains(text_filter))
                    continue;

                if (m_TimeStamps)
                    ImGui.PushTextWrapPos(ImGui.GetColumnWidth(0) - timestamp_width);

                // Spacing between commands.
                if (item.severity == Severity.Command) {
                    // Wrap before timestamps start.
                    if (count++ != 0)
                        ImGui.Dummy(new(-1, ImGui.GetFontSize()));
                }

                // Items.
                if (m_ColoredOutput) {
                    ImGui.PushStyleColor(ImGuiCol.Text, consoleColorPalette[item.severity]);
                    ImGui.TextUnformatted(item.message);
                    ImGui.PopStyleColor();
                } else {
                    ImGui.TextUnformatted(item.message);
                }

                // Time stamp.
                if (m_TimeStamps) {
                    // No wrap for timestamps
                    ImGui.PopTextWrapPos();

                    // Right align.
                    ImGui.SameLine(ImGui.GetColumnWidth(-1) - timestamp_width, 0);

                    // Draw time stamp.
                    ImGui.PushStyleColor(ImGuiCol.Text, consoleColorPalette[Severity.Timestamp]);
                    ImGui.Text(item.timestamp);
                    ImGui.PopStyleColor();
                }
            }

            // Stop wrapping since we are done displaying console items.
            //if (!m_TimeStamps)
            //    ImGui.PopTextWrapPos();

            // Auto-scroll logs.
            if (m_ScrollToBottom && (ImGui.GetScrollY() >= ImGui.GetScrollMaxY() || m_AutoScroll))
                ImGui.SetScrollHereY(1.0f);

            m_ScrollToBottom = false;
        }

        ImGui.EndChild();
    }

    private static unsafe void input_bar() {
        // Variables.
        ImGuiInputTextFlags inputTextFlags =
                ImGuiInputTextFlags.CallbackHistory | ImGuiInputTextFlags.CallbackCharFilter | ImGuiInputTextFlags.CallbackCompletion |
                ImGuiInputTextFlags.EnterReturnsTrue;

        // Only reclaim after enter key is pressed!
        bool reclaimFocus = false;

        // Input widget. (Width an always fixed width)
        ImGui.PushItemWidth(ImGui.GetColumnWidth());
        if (ImGui.InputText("##Input", ref m_Buffer, m_Buffer_size, inputTextFlags, input_callback)) {
            // Validate.
            if (!string.IsNullOrWhiteSpace(m_Buffer)) {
                string[] args = m_Buffer.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                // Run command line input.
                if (commands.TryGetValue(args[0], out var command)) {
                    command(args.Skip(1).ToArray());
                } else {
                    log(Severity.Error, "command not found");
                }

                // Scroll to bottom after its ran.
                m_ScrollToBottom = true;
            }

            // Keep focus.
            reclaimFocus = true;

            // Clear command line.
            m_Buffer = new(new char[m_Buffer.Length]);
        }

        ImGui.PopItemWidth();

        // Reset suggestions when client provides char input.
        if (ImGui.IsItemEdited() && !m_WasPrevFrameTabCompletion)
            m_CmdSuggestions.Clear();

        m_WasPrevFrameTabCompletion = false;

        // Auto-focus on window apparition
        ImGui.SetItemDefaultFocus();
        if (reclaimFocus)
            ImGui.SetKeyboardFocusHere(-1); // Focus on command line after clearing.
    }

    private static void help_marker(string desc) {
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(desc);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    private static void tool_bar() {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(10, 4)); // Add some spacing between toolbar items

        // Begin toolbar area
        ImGui.BeginGroup();

        // Settings button
        if (ImGui.Button("Settings")) {
            ImGui.OpenPopup("SettingsPopup");
        }

        ImGui.SameLine();

        if (ImGui.Button("Appearance")) {
            ImGui.OpenPopup("AppearancePopup");
        }

        // View settings.
        if (ImGui.BeginPopup("AppearancePopup")) {
            // Logging Colors
            ImGuiColorEditFlags flags =
                ImGuiColorEditFlags.Float | ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar;

            ImGui.ColorEdit4("Command##", ref consoleColorPalette[Severity.Command], flags);
            ImGui.SameLine();
            ImGui.ColorEdit4("Log##", ref consoleColorPalette[Severity.Log], flags);
            ImGui.SameLine();
            ImGui.ColorEdit4("Warning##", ref consoleColorPalette[Severity.Warning], flags);
            ImGui.SameLine();
            ImGui.ColorEdit4("Error##", ref consoleColorPalette[Severity.Error], flags);
            ImGui.SameLine();
            ImGui.ColorEdit4("Info##", ref consoleColorPalette[Severity.Info], flags);
            ImGui.SameLine();
            ImGui.ColorEdit4("Time Stamp##", ref consoleColorPalette[Severity.Timestamp], flags);

            ImGui.Separator();

            ImGui.Text("Transparency");
            ImGui.SameLine();

            ImGui.SliderFloat("##Transparency", ref m_WindowAlpha, 0.1f, 1.0f);

            ImGui.EndPopup();
        }

        if (ImGui.BeginPopup("SettingsPopup")) {
            // Colored output
            ImGui.Checkbox("Colored Output", ref m_ColoredOutput);
            ImGui.SameLine();
            help_marker("Enable colored command output");

            // Auto Scroll
            ImGui.Checkbox("Auto Scroll", ref m_AutoScroll);
            ImGui.SameLine();
            help_marker("Automatically scroll to bottom of console log");

            // Time stamp
            ImGui.Checkbox("Time Stamps", ref m_TimeStamps);
            ImGui.SameLine();
            help_marker("Display command execution timestamps");

            // Reset to default settings
            if (ImGui.Button("Reset settings", new(ImGui.GetColumnWidth(0), 0))) {
                ImGui.OpenPopup("Reset Settings?");
            }

            // Confirmation
            if (ImGui.BeginPopupModal("Reset Settings?", ref m_resetModal, ImGuiWindowFlags.AlwaysAutoResize)) {
                ImGui.Text("All settings will be reset to default.\nThis operation cannot be undone!\n\n");
                ImGui.Separator();

                if (ImGui.Button("Reset", new(120, 0))) {
                    default_settings();
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SetItemDefaultFocus();
                ImGui.SameLine();
                if (ImGui.Button("Cancel", new Vector2(120, 0)))
                    ImGui.CloseCurrentPopup();

                ImGui.EndPopup();
            }

            ImGui.EndPopup();
        }

        ImGui.SameLine();

        ImGui.InputText("Filter", ref text_filter, (nuint)(ImGui.GetWindowWidth() * 0.25f), ImGuiInputTextFlags.None);

        ImGui.EndGroup(); // End toolbar area

        // Reset style to default
        ImGui.PopStyleVar();
    }

    private static unsafe int input_callback(ImGuiInputTextCallbackData* data) {
        // Exit if no buffer.
        if (data->BufTextLen == 0 && data->EventFlag != ImGuiInputTextFlags.CallbackHistory)
            return 0;

        // Get input string and console.
        string input_str = Encoding.UTF8.GetString(data->Buf, data->BufTextLen);
        string trim_str = input_str.Trim();

        int startPos = m_Buffer.IndexOf(' ');
        startPos = startPos == -1 ? 0 : startPos;
        int endPos = m_Buffer.LastIndexOf(' ');
        endPos = endPos == -1 ? m_Buffer.Length : endPos;

        Span<char> buffer = new(data->Buf, data->BufSize);

        switch (data->EventFlag) {
            case ImGuiInputTextFlags.CallbackCompletion:
                {
                    // Find last word.
                    int startSubtrPos = trim_str.LastIndexOf(' ');
                    startSubtrPos = startSubtrPos == -1 ? 0 : startSubtrPos;

                    // Validate str
                    if (!string.IsNullOrEmpty(trim_str)) {
                        // Display suggestions on console.
                        if (m_CmdSuggestions.Count != 0) {
                            log(Severity.Command, "Suggestions: ");
                            foreach (var suggestion in m_CmdSuggestions) {
                                log(Severity.Log, suggestion);
                            }

                            m_CmdSuggestions.Clear();
                        }

                        // Get partial completion and suggestions.
                        string partial = trim_str.Substring(startSubtrPos, endPos);
                        m_CmdSuggestions.AddRange(cmdAutocomplete.StartingWith(partial).Select(x => x.Key));

                        // Autocomplete only when one work is available.
                        if (m_CmdSuggestions.Count != 0 && m_CmdSuggestions.Count == 1) {
                            buffer[startSubtrPos..data->BufTextLen].Fill((char)0);
                            string ne = m_CmdSuggestions[0];
                            m_CmdSuggestions.Clear();
                            data->Buf = (byte*)Marshal.StringToCoTaskMemUTF8(ne).ToPointer();
                            data->BufTextLen = ne.Length;
                            data->CursorPos = ne.Length;
                            data->BufDirty = 1;
                        } else {
                            // Partially complete word.
                            if (!string.IsNullOrEmpty(partial)) {
                                int newLen = data->BufTextLen - startSubtrPos;
                                buffer[startSubtrPos..data->BufTextLen].Fill((char)0);
                                partial.CopyTo(buffer[startSubtrPos..]);
                                data->BufDirty = 1;
                            }
                        }
                    }

                    // We have performed the completion event.
                    m_WasPrevFrameTabCompletion = true;
                }
                break;

            case ImGuiInputTextFlags.CallbackHistory:
                {
                    // Clear buffer.
                    data->BufTextLen = 0;

                    // Traverse history.
                    if (data->EventKey == ImGuiKey.UpArrow) {
                        if (m_HistoryIndex > 0) {
                            --m_HistoryIndex;
                        }
                    } else {
                        if (m_HistoryIndex < history.Count) {
                            ++m_HistoryIndex;
                        }
                    }

                    // Get history.
                    string prevCommand = history[m_HistoryIndex];

                    // Insert commands.
                    Unsafe.Copy(data->Buf, ref prevCommand);
                    data->BufTextLen = prevCommand.Length;
                }
                break;

            case ImGuiInputTextFlags.CallbackCharFilter:
            case ImGuiInputTextFlags.CallbackAlways:
            default:
                break;
        }
        return 1;
    }

    public readonly struct Message {
        public readonly Severity severity;
        public readonly string message;
        public readonly string timestamp;

        public Message(Severity severity, string source, string message) {
            this.severity = severity;
            this.message = $"[{source}]: {message}";
            this.timestamp = DateTime.Now.ToLongTimeString();
        }

        public Message(Severity severity, string message) {
            this.severity = severity;
            this.message = message;
            this.timestamp = DateTime.Now.ToLongTimeString();
        }
    }

    public enum Severity {
        Command,
        Log,
        Warning,
        Error,
        Info,
        Timestamp,
        Critical
    }

    private sealed class ColorPalette {
        private readonly Vector4[] values;

        public ColorPalette() {
            values = new Vector4[Enum.GetValues<DebugConsole.Severity>().Length];
            this[DebugConsole.Severity.Command] = new(1.0f, 1.0f, 1.0f, 1.0f);
            this[DebugConsole.Severity.Log] = new(1.0f, 1.0f, 1.0f, 0.5f);
            this[DebugConsole.Severity.Warning] = new(1.0f, 0.87f, 0.37f, 1.0f);
            this[DebugConsole.Severity.Error] = new(1.0f, 0.365f, 0.365f, 1.0f);
            this[DebugConsole.Severity.Info] = new(0.46f, 0.96f, 0.46f, 1.0f);
            this[DebugConsole.Severity.Timestamp] = new(1.0f, 1.0f, 1.0f, 0.5f);
        }

        public ref Vector4 this[DebugConsole.Severity index] => ref values[(int)index];
    }
}