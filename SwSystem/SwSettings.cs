﻿using Android.Util;
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
        private static string KEYMAP_SELECTION_FILENAME = "SelectedKeymap.txt";
        private static string KEYMAP_SELECTION_PATH = Path.Combine(FileSystem.AppDataDirectory, KEYMAP_SELECTION_FILENAME);
        private static string KEYMAP_DIRECTORY = Path.Combine(FileSystem.AppDataDirectory, "keymap_3");
        public static List<string> KEYMAP_FILENAMES = new List<string> { "Profile1", "Profile2", "Profile3", "Profile4", "Profile5" };
        private static string OPEN_SOUND_FILENAME = "open_sound.txt";
        private static string CLOSE_SOUND_FILENAME = "close_sound.txt";
        private static string OPEN_SOUND_RECORD_PATH = Path.Combine(FileSystem.AppDataDirectory, OPEN_SOUND_FILENAME);
        private static string CLOSE_SOUND_RECORD_PATH = Path.Combine(FileSystem.AppDataDirectory, CLOSE_SOUND_FILENAME);
        private static string VIBRATION_ENABLE_FILENAME = "vibration_enable.txt";
        private static string VIBRATION_ENABLE_PATH = Path.Combine(FileSystem.AppDataDirectory, VIBRATION_ENABLE_FILENAME);
        private static string LAYOUT_SELECTION_FILENAME = "SelectedLayout.txt";
        private static string LAYOUT_SELECTION_PATH = Path.Combine(FileSystem.AppDataDirectory, LAYOUT_SELECTION_FILENAME);
        private const string CONTROL_KEY_DELIMITER = "=";
        private const string MOTION_DELIMITER = ",";
        private static bool local_keymap_updated = false;
        public const bool Default_Vibration_Enable = true;
        public static MotionDescription Default_Motion = MotionDescription.InvalidMotion();
        public static int Default_Layout = Resource.Layout.input_a;
        public enum ControlId : int
        {
            Unknown,
            All,
            L_Button,
            R_Button,
            L_Analog,
            R_Analog,
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
        public static Dictionary<string, int> LAYOUT_TO_STRING_MAP = new Dictionary<string, int>
        {
            { "Layout A", Resource.Layout.input_a },
            { "Layout B", Resource.Layout.input_b },
            { "Layout C", Resource.Layout.input_c },
        };
        public static Dictionary<string, int> DIRECTION_TO_STRING_MAP = new Dictionary<string, int>
        {
            { "4-way", 4 },
            { "8-way", 8 },
            { "12-way", 12 },
            { "16-way", 16 },
            { "24-way", 24 },
            { "360 degrees", 360 },
        };
        public static Dictionary<string, bool> VIBRATION_TO_STRING_MAP = new Dictionary<string, bool>
        {
            { "Enable" , true },
            { "Disable", false}
        };
        public static Dictionary<ControlId, string> CONTROL_TO_STRING_MAP = new Dictionary<ControlId, string>
        {
            { ControlId.L_Button        , "Left Shoulder Button" },
            { ControlId.R_Button        , "Right Shoulder Button"},
            { ControlId.L_Analog        , "Left Analog"          },
            { ControlId.R_Analog        , "Right Analog"         },
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
        private static Dictionary<ControlId, MotionDescription> CONTROL_TO_MOTION_MAP = new Dictionary<ControlId, MotionDescription>
        {
            { ControlId.L_Button        , Default_Motion},
            { ControlId.R_Button        , Default_Motion},
            { ControlId.L_Analog        , Default_Motion},
            { ControlId.R_Analog        , Default_Motion},
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
        public static Dictionary<int, ControlId> RESOURCE_TO_CONTROL_MAP = new Dictionary<int, ControlId>
        {
            {Resource.Id.l_button,          ControlId.L_Button    },
            {Resource.Id.r_button,          ControlId.R_Button    },
            {Resource.Id.left_joyStick,     ControlId.L_Analog    },
            {Resource.Id.right_joyStick,    ControlId.R_Analog    },
            {Resource.Id.x_button,          ControlId.X_Button    },
            {Resource.Id.y_button,          ControlId.Y_Button    },
            {Resource.Id.a_button,          ControlId.A_Button    },
            {Resource.Id.b_button,          ControlId.B_Button    },
            {Resource.Id.start_button,      ControlId.Start_Button},
            {Resource.Id.d_pad_up,          ControlId.D_Pad_Up    },
            {Resource.Id.d_pad_down,        ControlId.D_Pad_Down  },
            {Resource.Id.d_pad_left,        ControlId.D_Pad_Left  },
            {Resource.Id.d_pad_right,       ControlId.D_Pad_Right },
            {Resource.Id.d_pad_center,      ControlId.D_Pad_Center}
        };

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
                var sound_file = reader.ReadLine().Replace("\n", "").Replace("\r", "");
                return File.Exists(sound_file) ? sound_file : "";
            }
        }

        public static void SetOpenSoundPath(string path)
        {
            Log.Debug(TAG, "SetOpenSoundPath");
            using (var writer = File.CreateText(OPEN_SOUND_RECORD_PATH))
            {
                writer.WriteLine(path);
            }
        }

        public static void SetCloseSoundPath(string path)
        {
            Log.Debug(TAG, "SetCloseSoundPath");
            using (var writer = File.CreateText(CLOSE_SOUND_RECORD_PATH))
            {
                writer.WriteLine(path);
            }
        }

        public static bool GetVibrationEnable()
        {
            Log.Debug(TAG, "GetVibrationEnable");
            if (!File.Exists(VIBRATION_ENABLE_PATH))
            {
                Log.Debug(TAG, "Vibration record not found");
                return Default_Vibration_Enable;
            }
            var stream = File.OpenRead(VIBRATION_ENABLE_PATH);
            using (var reader = new StreamReader(stream))
            {
                var enableStr = reader.ReadLine().Replace("\n", "").Replace("\r", "");
                if (bool.TryParse(enableStr, out bool enable))
                {
                    return enable;
                }
                return Default_Vibration_Enable;
            }
        }

        public static void SetVibrationEnable(bool enable)
        {
            Log.Debug(TAG, "SetVibrationEnable");
            using (var writer = File.CreateText(VIBRATION_ENABLE_PATH))
            {
                writer.WriteLine(enable.ToString());
            }
        }

        public static string GetSelectedKeymap()
        {
            Log.Debug(TAG, "GetSelectedKeymap");
            if (!File.Exists(KEYMAP_SELECTION_PATH))
            {
                Log.Debug(TAG, "Selected keymap record not found");
                return KEYMAP_FILENAMES[0];
            }
            var stream = File.OpenRead(KEYMAP_SELECTION_PATH);
            using (var reader = new StreamReader(stream))
            {
                var keymapStr = reader.ReadLine().Replace("\n", "").Replace("\r", "");
                return keymapStr;
            }
        }

        public static void SetSelectedKeymap(string keymapName)
        {
            Log.Debug(TAG, "SetSelectedKeymap");
            using (var writer = File.CreateText(KEYMAP_SELECTION_PATH))
            {
                writer.WriteLine(keymapName);
            }

            // If the keymap does not exist yet, initialize it
            if (!Directory.Exists(KEYMAP_DIRECTORY))
            {
                Directory.CreateDirectory(KEYMAP_DIRECTORY);
            }
            local_keymap_updated = false;
            UpdateLocalKeymap();
        }

        public static int GetSelectedLayout()
        {
            Log.Debug(TAG, "GetSelectedLayout");
            if (!File.Exists(LAYOUT_SELECTION_PATH))
            {
                Log.Debug(TAG, "Selected layout record not found");
                return Default_Layout;
            }
            var stream = File.OpenRead(LAYOUT_SELECTION_PATH);
            using (var reader = new StreamReader(stream))
            {
                var layoutStr = reader.ReadLine().Replace("\n", "").Replace("\r", "");
                return int.Parse(layoutStr);
            }
        }

        public static void SetSelectedLayout(int layoutId)
        {
            Log.Debug(TAG, "SetSelectedLayout");
            using (var writer = File.CreateText(LAYOUT_SELECTION_PATH))
            {
                writer.WriteLine(layoutId.ToString());
            }
        }

        public static void SetControlMotion(ControlId control, MotionDescription motion)
        {
            CONTROL_TO_MOTION_MAP[control] = motion;
            UpdateStoredKeymap();
        }

        public static MotionDescription GetControlMotion(ControlId control)
        {
            UpdateLocalKeymap();
            return CONTROL_TO_MOTION_MAP[control];
        }

        private static void ResetLocalKeymap()
        {
            var control_keys = CONTROL_TO_STRING_MAP.Keys;
            foreach (var control in control_keys)
            {
                CONTROL_TO_MOTION_MAP[control] = Default_Motion;
            }
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

            ResetLocalKeymap();
            var keymapName = GetSelectedKeymap();
            if (!Directory.Exists(KEYMAP_DIRECTORY))
            {
                Directory.CreateDirectory(KEYMAP_DIRECTORY);
            }
            var keymapPath = Path.Combine(KEYMAP_DIRECTORY, keymapName);
            if (!File.Exists(keymapPath))
            {
                Log.Debug(TAG, "Keymap not found");
                return;
            }
            try
            {
                var stream = File.OpenRead(keymapPath);
                using (var reader = new StreamReader(stream))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        Log.Debug(TAG, line);
                        var control_key_str = line.Split(CONTROL_KEY_DELIMITER);
                        var control = GetControlFromString(control_key_str[0]);
                        CONTROL_TO_MOTION_MAP[control] = GetMotionFromString(control_key_str[1]);

                        line = reader.ReadLine();
                    }
                }
            }
            catch
            {
                Log.Debug(TAG, "File format invalid! Deleting...");
                File.Delete(keymapPath);
            }
        }

        private static void UpdateStoredKeymap()
        {
            Log.Debug(TAG, "UpdateStoredKeymap");
            var keymapName = GetSelectedKeymap();
            if (!Directory.Exists(KEYMAP_DIRECTORY))
            {
                Directory.CreateDirectory(KEYMAP_DIRECTORY);
            }
            var keymapPath = Path.Combine(KEYMAP_DIRECTORY, keymapName);
            using (var writer = File.CreateText(keymapPath))
            {
                foreach (var control in CONTROL_TO_STRING_MAP.Keys)
                {
                    MotionDescription motion;
                    CONTROL_TO_MOTION_MAP.TryGetValue(control, out motion);

                    var writetext = CONTROL_TO_STRING_MAP[control] + CONTROL_KEY_DELIMITER + GetStringFromMotion(motion);
                    writer.WriteLine(writetext);
                }
            }
        }

        private static string GetStringFromMotion(MotionDescription motion)
        {
            return motion.beginX.ToString() + MOTION_DELIMITER +
                motion.beginY.ToString() + MOTION_DELIMITER +
                motion.endX.ToString() + MOTION_DELIMITER +
                motion.endY.ToString() + MOTION_DELIMITER +
                ((Int32)motion.directionCount).ToString() + MOTION_DELIMITER +
                ((Int32)motion.type).ToString();
        }

        private static MotionDescription GetMotionFromString(string motionString)
        {
            MotionDescription output = MotionDescription.InvalidMotion();
            var motions = motionString.Split(MOTION_DELIMITER);
            output.beginX = float.Parse(motions[0]);
            output.beginY = float.Parse(motions[1]);
            output.endX = float.Parse(motions[2]);
            output.endY = float.Parse(motions[3]);
            output.directionCount = Int32.Parse(motions[4]);
            output.type = (MotionType)Int32.Parse(motions[5]);

            return output;
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