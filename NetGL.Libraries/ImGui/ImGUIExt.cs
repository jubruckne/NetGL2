using System.Numerics;

namespace ImGuiNET;

public static partial class ImGui {
    private class JoystickData {
        public bool is_dragging;
        public Vector2 initial_angles;
        public Vector2 drag_start_mouse_position;

        private JoystickData() { }

        public static JoystickData make(bool is_dragging = false, Vector2 initial_angles = default, Vector2 drag_start_mouse_position = default) {
            return new JoystickData {
                is_dragging = is_dragging,
                initial_angles = initial_angles,
                drag_start_mouse_position = drag_start_mouse_position
            };
        }
    }

    private static Dictionary<string, JoystickData> joystick_data_list = [];

    public static void Joystick(Vector3 transform, float radius = 50f) {
        /*

        ImGui.SeparatorText("Joystick 3");
        ImGui.Text($"yaw:{transform.attitude.yaw}, pitch:{transform.attitude.pitch}, roll:{transform.attitude.roll}");

        // Establish the base position and size for the joystick control
        Vector2 joystickBasePos = ImGui.GetCursorScreenPos(); // Top-left corner of the joystick area
        ImGui.InvisibleButton("joystick", new Vector2(radius * 2f, radius * 2f));

        bool isDragging = ImGui.IsItemActive(); // Check if the joystick is being interacted with
        Vector2 mousePos = new Vector2(ImGui.GetMousePos().X, ImGui.GetMousePos().Y);
        Vector2 centerPos = joystickBasePos + new Vector2(radius, radius); // Center of the joystick area

        Vector2 handlePos = centerPos; // Default position of the joystick handle (inner circle) is the center

        if (isDragging) {
            Vector2 dragVec = mousePos - centerPos;

            // Clamp the drag vector within the joystick radius to ensure the handle doesn't exit the joystick area
            if (dragVec.Length() > radius) {
                dragVec = Vector2.Normalize(dragVec) * radius;
            }

            handlePos = centerPos + dragVec; // Update the handle position based on the clamped drag vector

            transform.attitude.yaw = -dragVec.X / radius * 180f;
            transform.attitude.pitch = -dragVec.Y / radius * 180f;
        }

        // Draw the joystick area (outer circle) and handle (inner circle) for visual feedback
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        drawList.AddCircle(centerPos, radius, ImGui.GetColorU32(ImGuiCol.Button), 16); // Outer circle
        drawList.AddCircleFilled(handlePos, 7.5f, ImGui.GetColorU32(ImGuiCol.ButtonHovered)); // Inner circle (handle)
        ImGui.PopID();
        */
    }

    public static void Joystick2(Vector3 transform, float radius = 50f) {
       /* if(!joystick_data_list.TryGetValue(((IComponent)transform).path, out var joystick_data)) {
            joystick_data = JoystickData.make();
            joystick_data_list.Add(((IComponent)transform).path, joystick_data);
        }

        joystick_data.is_dragging = true;

        ImGui.PushID($"{transform.entity.path}.joy");
        ImGui.SeparatorText("Joystick 3");
        ImGui.Text($"yaw:{transform.attitude.yaw:F1}, pitch:{transform.attitude.pitch:F1}, roll:{transform.attitude.roll:F1}");

        Vector2 joystickBasePos = ImGui.GetCursorScreenPos();
        ImGui.InvisibleButton("joystick", new Vector2(radius * 2f, radius * 2f));

        Vector2 centerPos = joystickBasePos + new Vector2(radius, radius);
        Vector2 mousePos = new Vector2(ImGui.GetMousePos().X, ImGui.GetMousePos().Y);

        if (ImGui.IsItemActive() && !joystick_data.is_dragging)
        {
            // When the joystick is first activated, record the start position and the initial angles
            joystick_data.is_dragging = true;
            joystick_data.drag_start_mouse_position = mousePos;
            joystick_data.initial_angles = new Vector2(transform.attitude.yaw, transform.attitude.pitch);
        }
        else if (!ImGui.IsItemActive())
        {
            joystick_data.is_dragging = false;
        }

        Vector2 handlePos = centerPos; // Default to center position

        if (joystick_data.is_dragging)
        {
            // Calculate drag vector from the initial drag start position
            Vector2 dragVec = mousePos - centerPos;

            if (dragVec.Length() > radius)
            {
                dragVec = Vector2.Normalize(dragVec) * radius;
            }

            handlePos = centerPos + dragVec;

            // Adjust rotation based on initial angles and drag displacement
            Vector2 angleChange = (dragVec - (joystick_data.drag_start_mouse_position - centerPos)) / radius * 180f;
            transform.attitude.yaw = joystick_data.initial_angles.X - angleChange.X * 0.75f;
            transform.attitude.pitch = joystick_data.initial_angles.Y - angleChange.Y * 0.75f; // Assuming Y-axis inversion for pitch control
        }

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        drawList.AddCircle(centerPos, radius, ImGui.GetColorU32(ImGuiCol.Button), 16);
        drawList.AddCircleFilled(handlePos, 7.5f, ImGui.GetColorU32(ImGuiCol.ButtonHovered));
        ImGui.PopID();
    */

    }
}