using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftWing.System
{
    public static class KeymapStorage
    {
        private static string KEYMAP_PATH = Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "keymap.txt");
        private const string CONTROL_KEY_DELIMITER = "=";
        private static bool local_keymap_updated = false;
        public enum ControlId
        {
            L_Button,
            R_Button,
            L_Analog_Up,
            L_Analog_Down,
            L_Analog_Left,
            L_Analog_Right,
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
        public static Dictionary<ControlId, string> CONTROL_TO_STRING_MAP = new Dictionary<ControlId, string>
        {
            { ControlId.L_Button        , "Left Shoulder Button" },
            { ControlId.R_Button        , "Right Shoulder Button"},
            { ControlId.L_Analog_Up     , "Left Analog Up"       },
            { ControlId.L_Analog_Down   , "Left Analog Down"     },
            { ControlId.L_Analog_Left   , "Left Analog Left"     },
            { ControlId.L_Analog_Right  , "Left Analog Right"    },
            { ControlId.R_Analog_Up     , "Right Analog Up"      },
            { ControlId.R_Analog_Down   , "Right Analog Down"    },
            { ControlId.R_Analog_Left   , "Right Analog Left"    },
            { ControlId.R_Analog_Right  , "Right Analog Right"   },
            { ControlId.X_Button        , "X Button (Top)"       },
            { ControlId.Y_Button        , "Y Button (Left)"      },
            { ControlId.A_Button        , "A Button (Right)"     },
            { ControlId.B_Button        , "B Button (Bottom)"    },
            { ControlId.Start_Button    , "Start Button"         },
            { ControlId.D_Pad_Up        , "D-Pad Up"             },
            { ControlId.D_Pad_Down      , "D-Pad Down"           },
            { ControlId.D_Pad_Left      , "D-Pad Left"           },
            { ControlId.D_Pad_Right     , "D-Pad Right"          },
            { ControlId.D_Pad_Center    , "D-Pad Center"         }
        };
        public static Dictionary<string, Keycode> STRING_TO_KEYCODE_MAP = new Dictionary<string, Keycode>
        {
            { "A", Keycode.A },
            { "B", Keycode.B },
            { "C", Keycode.C },
            { "D", Keycode.D },
            { "E", Keycode.E },
            { "F", Keycode.F },
            { "G", Keycode.G },
            { "H", Keycode.H },
            { "I", Keycode.I },
            { "J", Keycode.J },
            { "K", Keycode.K },
            { "L", Keycode.L },
            { "M", Keycode.M },
            { "N", Keycode.N },
            { "O", Keycode.O },
            { "P", Keycode.P },
            { "Q", Keycode.Q },
            { "R", Keycode.R },
            { "S", Keycode.S },
            { "T", Keycode.T },
            { "U", Keycode.U },
            { "V", Keycode.V },
            { "W", Keycode.W },
            { "X", Keycode.X },
            { "Y", Keycode.Y },
            { "Z", Keycode.Z },
            { "Escape", Keycode.Escape },
            { "Tab", Keycode.Tab },
            { "Enter", Keycode.Enter},
            { "Space", Keycode.Space },
            { "D-Pad Up", Keycode.DpadUp },
            { "D-Pad Down", Keycode.DpadDown },
            { "D-Pad Left", Keycode.DpadLeft },
            { "D-Pad Right", Keycode.DpadRight },
            { "D-Pad Center", Keycode.DpadCenter },
            { "Button 1", Keycode.Button1 },
            { "Button 2", Keycode.Button2 },
            { "Button 3", Keycode.Button3 },
            { "Button 4", Keycode.Button4 },
            { "Button A", Keycode.ButtonA },
            { "Button B", Keycode.ButtonB },
            { "Button X", Keycode.ButtonX },
            { "Button Y", Keycode.ButtonY },
            { "Button Z", Keycode.ButtonZ },
            { "Button L1", Keycode.ButtonL1 },
            { "Button L2", Keycode.ButtonL2 },
            { "Button R1", Keycode.ButtonR1 },
            { "Button R2", Keycode.ButtonR2 },
            { "Button SELECT", Keycode.ButtonSelect },
            { "Button START", Keycode.ButtonStart }
        };
        private static Dictionary<ControlId, Keycode> CONTROL_TO_KEY_MAP = new Dictionary<ControlId, Keycode>
        {
            { ControlId.L_Button, Keycode.ButtonL1 },
            { ControlId.R_Button, Keycode.ButtonR1 },
            { ControlId.L_Analog_Up, Keycode.W },
            { ControlId.L_Analog_Down, Keycode.S },
            { ControlId.L_Analog_Left, Keycode.A },
            { ControlId.L_Analog_Right, Keycode.D },
            { ControlId.R_Analog_Up, Keycode.Button1 },
            { ControlId.R_Analog_Down, Keycode.Button2 },
            { ControlId.R_Analog_Left, Keycode.Button3 },
            { ControlId.R_Analog_Right, Keycode.Button4 },
            { ControlId.X_Button, Keycode.ButtonX },
            { ControlId.Y_Button, Keycode.ButtonY },
            { ControlId.A_Button, Keycode.ButtonA },
            { ControlId.B_Button, Keycode.ButtonB },
            { ControlId.Start_Button, Keycode.ButtonStart },
            { ControlId.D_Pad_Up, Keycode.DpadUp },
            { ControlId.D_Pad_Down, Keycode.DpadDown },
            { ControlId.D_Pad_Left, Keycode.DpadLeft },
            { ControlId.D_Pad_Right, Keycode.DpadRight },
            { ControlId.D_Pad_Center, Keycode.DpadCenter }
        };

        public static void SetControlKeycode(ControlId control, Keycode key)
        {
            CONTROL_TO_KEY_MAP[control] = key;
            UpdateStoredKeymap().Wait();
        }
        public static void GetControlKeycode(ControlId control, Keycode key)
        {
            UpdateLocalKeymap().Wait();
            CONTROL_TO_KEY_MAP[control] = key;
        }

        private static async Task UpdateLocalKeymap()
        {
            // Only need to update from storage once per session
            if (local_keymap_updated)
            {
                return;
            }
            if (!File.Exists(KEYMAP_PATH))
            {
                return;
            }
            local_keymap_updated = true;
            using (var reader = new StreamReader(KEYMAP_PATH, true))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var control_key_str = line.Split(CONTROL_KEY_DELIMITER);
                    var control = (ControlId)Int32.Parse(control_key_str[0]);
                    var key = (Keycode)Int32.Parse(control_key_str[1]);
                    CONTROL_TO_KEY_MAP[control] = key;
                }
            }
        }

        private static async Task UpdateStoredKeymap()
        {
            using (var writer = File.CreateText(KEYMAP_PATH))
            {
                foreach (var control in CONTROL_TO_KEY_MAP.Keys)
                {
                    Keycode key;
                    CONTROL_TO_KEY_MAP.TryGetValue(control, out key);
                    await writer.WriteLineAsync(control.ToString() + CONTROL_KEY_DELIMITER + key.ToString());
                }
            }
        }
    }
}