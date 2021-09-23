using Android.Util;
using Android.Views;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Essentials;
using SoftWing.SwSystem.Messages;

namespace SoftWing.SwSystem
{
    public static class SwSettings
    {
        private static string TAG = "KeymapStorage";
        private static string KEYMAP_FILENAME = "keymap.txt";
        private static string KEYMAP_PATH = Path.Combine(FileSystem.AppDataDirectory, KEYMAP_FILENAME);
        private static string OPEN_SOUND_FILENAME = "open_sound.txt";
        private static string CLOSE_SOUND_FILENAME = "close_sound.txt";
        private static string OPEN_SOUND_RECORD_PATH = Path.Combine(FileSystem.AppDataDirectory, OPEN_SOUND_FILENAME);
        private static string CLOSE_SOUND_RECORD_PATH = Path.Combine(FileSystem.AppDataDirectory, CLOSE_SOUND_FILENAME);
        private static string TRANSITION_DELAY_FILENAME = "transition_delay.txt";
        private static string TRANSITION_DELAY_PATH = Path.Combine(FileSystem.AppDataDirectory, TRANSITION_DELAY_FILENAME);
        private const string CONTROL_KEY_DELIMITER = "=";
        private const string MOTION_DELIMITER = ",";
        private static bool local_keymap_updated = false;
        public const Keycode Default_L_Button = Keycode.ButtonL1;
        public const Keycode Default_R_Button = Keycode.ButtonR1;
        public const Keycode Default_L_Analog_Up = Keycode.W;
        public const Keycode Default_L_Analog_Down = Keycode.S;
        public const Keycode Default_L_Analog_Left = Keycode.A;
        public const Keycode Default_L_Analog_Right = Keycode.D;
        public const Keycode Default_R_Analog_Up = Keycode.Button1;
        public const Keycode Default_R_Analog_Down = Keycode.Button2;
        public const Keycode Default_R_Analog_Left = Keycode.Button3;
        public const Keycode Default_R_Analog_Right = Keycode.Button4;
        public const Keycode Default_X_Button = Keycode.ButtonX;
        public const Keycode Default_Y_Button = Keycode.ButtonY;
        public const Keycode Default_A_Button = Keycode.ButtonA;
        public const Keycode Default_B_Button = Keycode.ButtonB;
        public const Keycode Default_Start_Button = Keycode.ButtonStart;
        public const Keycode Default_D_Pad_Up = Keycode.DpadUp;
        public const Keycode Default_D_Pad_Down = Keycode.DpadDown;
        public const Keycode Default_D_Pad_Left = Keycode.DpadLeft;
        public const Keycode Default_D_Pad_Right = Keycode.DpadRight;
        public const Keycode Default_D_Pad_Center = Keycode.DpadCenter;
        public const int Default_Transition_Delay_Ms = 500;
        public static MotionDescription Default_Motion = MotionDescription.InvalidMotion();
        public enum ControlId : int
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
        public static Dictionary<string, int> DELAY_TO_STRING_MAP = new Dictionary<string, int>
        {
            { "0"   , 0 },
            { "0.5" , 500},
            { "1"   , 1000},
            { "1.5" , 1500},
            { "2"   , 2000},
            { "2.5" , 2500},
            { "3"   , 3000}
        };
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
            { "Button START", Keycode.ButtonStart },
            { "Touch Control", Keycode.Unknown }
        };
        private static Dictionary<ControlId, Keycode> CONTROL_TO_KEY_MAP = new Dictionary<ControlId, Keycode>
        {
            { ControlId.L_Button        , Default_L_Button       },
            { ControlId.R_Button        , Default_R_Button       },
            { ControlId.L_Analog_Up     , Default_L_Analog_Up    },
            { ControlId.L_Analog_Down   , Default_L_Analog_Down  },
            { ControlId.L_Analog_Left   , Default_L_Analog_Left  },
            { ControlId.L_Analog_Right  , Default_L_Analog_Right },
            { ControlId.R_Analog_Up     , Default_R_Analog_Up    },
            { ControlId.R_Analog_Down   , Default_R_Analog_Down  },
            { ControlId.R_Analog_Left   , Default_R_Analog_Left  },
            { ControlId.R_Analog_Right  , Default_R_Analog_Right },
            { ControlId.X_Button        , Default_X_Button       },
            { ControlId.Y_Button        , Default_Y_Button       },
            { ControlId.A_Button        , Default_A_Button       },
            { ControlId.B_Button        , Default_B_Button       },
            { ControlId.Start_Button    , Default_Start_Button   },
            { ControlId.D_Pad_Up        , Default_D_Pad_Up       },
            { ControlId.D_Pad_Down      , Default_D_Pad_Down     },
            { ControlId.D_Pad_Left      , Default_D_Pad_Left     },
            { ControlId.D_Pad_Right     , Default_D_Pad_Right    },
            { ControlId.D_Pad_Center    , Default_D_Pad_Center   }
        };
        private static Dictionary<ControlId, MotionDescription> CONTROL_TO_MOTION_MAP = new Dictionary<ControlId, MotionDescription>
        {
            { ControlId.L_Button        , Default_Motion},
            { ControlId.R_Button        , Default_Motion},
            { ControlId.L_Analog_Up     , Default_Motion},
            { ControlId.L_Analog_Down   , Default_Motion},
            { ControlId.L_Analog_Left   , Default_Motion},
            { ControlId.L_Analog_Right  , Default_Motion},
            { ControlId.R_Analog_Up     , Default_Motion},
            { ControlId.R_Analog_Down   , Default_Motion},
            { ControlId.R_Analog_Left   , Default_Motion},
            { ControlId.R_Analog_Right  , Default_Motion},
            { ControlId.X_Button        , Default_Motion},
            { ControlId.Y_Button        , Default_Motion},
            { ControlId.A_Button        , Default_Motion},
            { ControlId.B_Button        , Default_Motion},
            { ControlId.Start_Button    , Default_Motion},
            { ControlId.D_Pad_Up        , Default_Motion},
            { ControlId.D_Pad_Down      , Default_Motion},
            { ControlId.D_Pad_Left      , Default_Motion},
            { ControlId.D_Pad_Right     , Default_Motion},
            { ControlId.D_Pad_Center    , Default_Motion}
        };

        public static ControlId DefaultKeycodeToControlId(Keycode key)
        {
            switch (key)
            {
                case Default_L_Button:
                    return ControlId.L_Button;
                case Default_R_Button:
                    return ControlId.R_Button;
                case Default_L_Analog_Up:
                    return ControlId.L_Analog_Up;
                case Default_L_Analog_Down:
                    return ControlId.L_Analog_Down;
                case Default_L_Analog_Left:
                    return ControlId.L_Analog_Left;
                case Default_L_Analog_Right:
                    return ControlId.L_Analog_Right;
                case Default_R_Analog_Up:
                    return ControlId.R_Analog_Up;
                case Default_R_Analog_Down:
                    return ControlId.R_Analog_Down;
                case Default_R_Analog_Left:
                    return ControlId.R_Analog_Left;
                case Default_R_Analog_Right:
                    return ControlId.R_Analog_Right;
                case Default_X_Button:
                    return ControlId.X_Button;
                case Default_Y_Button:
                    return ControlId.Y_Button;
                case Default_A_Button:
                    return ControlId.A_Button;
                case Default_B_Button:
                    return ControlId.B_Button;
                case Default_D_Pad_Up:
                    return ControlId.D_Pad_Up;
                case Default_D_Pad_Down:
                    return ControlId.D_Pad_Down;
                case Default_D_Pad_Left:
                    return ControlId.D_Pad_Left;
                case Default_D_Pad_Right:
                    return ControlId.D_Pad_Right;
                case Default_D_Pad_Center:
                    return ControlId.D_Pad_Center;
                default: // Start Button
                    return ControlId.Start_Button;
            }
        }

        public static void SetDefaultKeycodes()
        {
            CONTROL_TO_KEY_MAP[ControlId.L_Button] = Default_L_Button;
            CONTROL_TO_KEY_MAP[ControlId.R_Button] = Default_R_Button;
            CONTROL_TO_KEY_MAP[ControlId.L_Analog_Up] = Default_L_Analog_Up;
            CONTROL_TO_KEY_MAP[ControlId.L_Analog_Down] = Default_L_Analog_Down;
            CONTROL_TO_KEY_MAP[ControlId.L_Analog_Left] = Default_L_Analog_Left;
            CONTROL_TO_KEY_MAP[ControlId.L_Analog_Right] = Default_L_Analog_Right;
            CONTROL_TO_KEY_MAP[ControlId.R_Analog_Up] = Default_R_Analog_Up;
            CONTROL_TO_KEY_MAP[ControlId.R_Analog_Down] = Default_R_Analog_Down;
            CONTROL_TO_KEY_MAP[ControlId.R_Analog_Left] = Default_R_Analog_Left;
            CONTROL_TO_KEY_MAP[ControlId.R_Analog_Right] = Default_R_Analog_Right;
            CONTROL_TO_KEY_MAP[ControlId.X_Button] = Default_X_Button;
            CONTROL_TO_KEY_MAP[ControlId.Y_Button] = Default_Y_Button;
            CONTROL_TO_KEY_MAP[ControlId.A_Button] = Default_A_Button;
            CONTROL_TO_KEY_MAP[ControlId.B_Button] = Default_B_Button;
            CONTROL_TO_KEY_MAP[ControlId.Start_Button] = Default_Start_Button;
            CONTROL_TO_KEY_MAP[ControlId.D_Pad_Up] = Default_D_Pad_Up;
            CONTROL_TO_KEY_MAP[ControlId.D_Pad_Down] = Default_D_Pad_Down;
            CONTROL_TO_KEY_MAP[ControlId.D_Pad_Left] = Default_D_Pad_Left;
            CONTROL_TO_KEY_MAP[ControlId.D_Pad_Right] = Default_D_Pad_Right;
            CONTROL_TO_KEY_MAP[ControlId.D_Pad_Center] = Default_D_Pad_Center;
            UpdateStoredKeymap();
        }

        public static string GetOpenSoundPath()
        {
            return GetSoundPath(OPEN_SOUND_RECORD_PATH);
        }

        public static string GetCloseSoundPath()
        {
            return GetSoundPath(CLOSE_SOUND_RECORD_PATH);
        }

        private static string GetSoundPath(string file_path)
        {
            Log.Debug(TAG, "GetSoundPath");
            if (!File.Exists(file_path))
            {
                Log.Debug(TAG, "Sound record not found");
                return "";
            }
            var stream = File.OpenRead(file_path);
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadLine().Replace("\n", "").Replace("\r", "");
            }
        }

        public static void SetOpenSoundPath(Android.Net.Uri path)
        {
            Log.Debug(TAG, "SetOpenSoundPath");
            using (var writer = File.CreateText(OPEN_SOUND_RECORD_PATH))
            {
                writer.WriteLine(path.ToString());
            }
        }

        public static int GetTransitionDelayMs()
        {
            Log.Debug(TAG, "GetTransitionDelayMs");
            if (!File.Exists(TRANSITION_DELAY_PATH))
            {
                Log.Debug(TAG, "Transition delay record not found");
                return Default_Transition_Delay_Ms;
            }
            var stream = File.OpenRead(TRANSITION_DELAY_PATH);
            using (var reader = new StreamReader(stream))
            {
                var delayStr = reader.ReadLine().Replace("\n", "").Replace("\r", "");
                if (Int32.TryParse(delayStr, out int delay))
                {
                    return delay;
                }
                return Default_Transition_Delay_Ms;
            }
        }

        public static void SetTransitionDelayMs(int delay)
        {
            Log.Debug(TAG, "SetTransitionDelayMs");
            using (var writer = File.CreateText(TRANSITION_DELAY_PATH))
            {
                writer.WriteLine(delay.ToString());
            }
        }

        public static void SetCloseSoundPath(Android.Net.Uri path)
        {
            Log.Debug(TAG, "SetCloseSoundPath");
            using (var writer = File.CreateText(CLOSE_SOUND_RECORD_PATH))
            {
                writer.WriteLine(path.ToString());
            }
        }

        public static void SetControlMotion(ControlId control, MotionDescription motion)
        {
            CONTROL_TO_KEY_MAP[control] = Keycode.Unknown;
            CONTROL_TO_MOTION_MAP[control] = motion;
            UpdateStoredKeymap();
        }

        public static void SetControlKeycode(ControlId control, Keycode key)
        {
            CONTROL_TO_KEY_MAP[control] = key;
            UpdateStoredKeymap();
        }

        public static Keycode GetControlKeycode(ControlId control)
        {
            UpdateLocalKeymap();
            return CONTROL_TO_KEY_MAP[control];
        }

        public static MotionDescription GetControlMotion(ControlId control)
        {
            UpdateLocalKeymap();
            return CONTROL_TO_MOTION_MAP[control];
        }

        private static void UpdateLocalKeymap()
        {
            Log.Debug(TAG, "UpdateLocalKeymap");
            // Only need to update from storage once per session
            if (local_keymap_updated)
            {
                Log.Debug(TAG, "Already updated");
                return;
            }
            local_keymap_updated = true;
            if (!File.Exists(KEYMAP_PATH))
            {
                Log.Debug(TAG, "Keymap not found");
                return;
            }
            var stream = File.OpenRead(KEYMAP_PATH);
            using (var reader = new StreamReader(stream))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    var control_key_str = line.Split(CONTROL_KEY_DELIMITER);
                    var control = GetControlFromString(control_key_str[0]);
                    var key = STRING_TO_KEYCODE_MAP[control_key_str[1]];
                    CONTROL_TO_KEY_MAP[control] = key;
                    if (control_key_str.Length > 2)
                    {
                        CONTROL_TO_MOTION_MAP[control] = GetMotionFromString(control_key_str[2]);
                    }

                    line = reader.ReadLine();
                }
            }
        }

        private static void UpdateStoredKeymap()
        {
            Log.Debug(TAG, "UpdateStoredKeymap");
            using (var writer = File.CreateText(KEYMAP_PATH))
            {
                foreach (var control in CONTROL_TO_KEY_MAP.Keys)
                {
                    Keycode key;
                    CONTROL_TO_KEY_MAP.TryGetValue(control, out key);
                    MotionDescription motion;
                    CONTROL_TO_MOTION_MAP.TryGetValue(control, out motion);

                    var writetext = CONTROL_TO_STRING_MAP[control] + CONTROL_KEY_DELIMITER + GetStringFromKeycode(key) + CONTROL_KEY_DELIMITER + GetStringFromMotion(motion);
                    writer.WriteLine(writetext);
                }
            }
        }

        private static string GetStringFromKeycode(Keycode key)
        {
            foreach (var key_string in STRING_TO_KEYCODE_MAP.Keys)
            {
                if (STRING_TO_KEYCODE_MAP[key_string] == key)
                {
                    return key_string;
                }
            }
            return "";
        }

        private static string GetStringFromMotion(MotionDescription motion)
        {
            return motion.beginX.ToString() + MOTION_DELIMITER +
                motion.beginY.ToString() + MOTION_DELIMITER +
                motion.endX.ToString() + MOTION_DELIMITER +
                motion.endY.ToString();
        }

        private static MotionDescription GetMotionFromString(string motionString)
        {
            float beginX = 0;
            float beginY = 0;
            float endX = 0;
            float endY = 0;
            try
            {
                var motions = motionString.Split(MOTION_DELIMITER);
                beginX = float.Parse(motions[0]);
                beginY = float.Parse(motions[1]);
                endX = float.Parse(motions[2]);
                endY = float.Parse(motions[3]);
            }
            catch (Exception e)
            {
                Log.Error(TAG, e.Message);
            }

            return new MotionDescription(-1, beginX, beginY, endX, endY);
        }

        private static ControlId GetControlFromString(string control_string)
        {
            foreach (var control in CONTROL_TO_STRING_MAP.Keys)
            {
                if (CONTROL_TO_STRING_MAP[control] == control_string)
                {
                    return control;
                }
            }
            return 0;
        }
    }
}