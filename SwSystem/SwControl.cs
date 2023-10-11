using Android.Views;
using System;

namespace SoftWing.SwSystem
{
    public enum ControlId : int
    {
        Unknown,
        L1_Button,
        L2_Button,
        R1_Button,
        R2_Button,
        L_Analog,
        L_Analog_Up,
        L_Analog_Down,
        L_Analog_Left,
        L_Analog_Right,
        R_Analog,
        R_Analog_Up,
        R_Analog_Down,
        R_Analog_Left,
        R_Analog_Right,
        X_Button,
        Y_Button,
        A_Button,
        B_Button,
        Start_Button,
        D_Pad_Up,
        D_Pad_Down,
        D_Pad_Left,
        D_Pad_Right,
        D_Pad_Center
    }
    public enum ButtonBehavior : int
    {
        Temporary, Toggle
    }
    public enum MotionType
    {
        Invalid = -1,
        Tap = 0,
        Swipe = 1,
        Continuous = 2,
    }

    public struct MotionDescription
    {
        public MotionType type { get; set; }
        public float beginX { get; set; }
        public float beginY { get; set; }
        public float endX { get; set; }
        public float endY { get; set; }
        public int directionCount { get; set; }

        public MotionDescription(MotionType type_in, float beginX_in, float beginY_in, float endX_in, float endY_in, int directionCount_in)
        {
            type = type_in;
            beginX = beginX_in;
            beginY = beginY_in;
            endX = endX_in;
            endY = endY_in;
            directionCount = directionCount_in;
        }

        public static MotionDescription InvalidMotion()
        {
            return new MotionDescription(MotionType.Invalid, -1, -1, -1, -1, 8);
        }

        public string Serialize()
        {
            return beginX.ToString() + "," +
                beginY.ToString() + "," +
                endX.ToString() + "," +
                endY.ToString() + "," +
                ((Int32)directionCount).ToString() + "," +
                ((Int32)type).ToString();
        }

        public static MotionDescription Deserialize(string serialized)
        {
            MotionDescription output = MotionDescription.InvalidMotion();
            var motions = serialized.Split(",");
            output.beginX = float.Parse(motions[0]);
            output.beginY = float.Parse(motions[1]);
            output.endX = float.Parse(motions[2]);
            output.endY = float.Parse(motions[3]);
            output.directionCount = Int32.Parse(motions[4]);
            output.type = (MotionType)Int32.Parse(motions[5]);

            return output;
        }

    }

    public class SwControl
    {
        public ControlId id { get; set; }
        public MotionDescription motion { get; set; }
        public Keycode key { get; set; }
        public ButtonBehavior behavior { get; set; }

        public SwControl(ControlId id_in, ButtonBehavior behavior_in = ButtonBehavior.Temporary, Keycode key_in = Keycode.Unknown, MotionDescription? motion_in = null)
        {
            id = id_in;
            behavior = behavior_in;
            key = key_in;
            if (motion_in != null)
            {
                motion = (MotionDescription)motion_in;
            }
            else
            {
                motion = MotionDescription.InvalidMotion();
            }
        }

        public string Serialize()
        {
            return id.ToString() + ";" + key.ToString() + ";" + motion.Serialize() + ";" + behavior.ToString();
        }

        public static SwControl Deserialize(string serialized)
        {
            var output = new SwControl(ControlId.Unknown);

            var control_entries = serialized.Split(";");
            output.id = Enum.Parse<ControlId>(control_entries[0]);
            output.key = Enum.Parse<Keycode>(control_entries[1]);
            output.motion = MotionDescription.Deserialize(control_entries[2]);
            output.behavior = Enum.Parse<ButtonBehavior>(control_entries[3]);
            return output;
        }
    }
}
