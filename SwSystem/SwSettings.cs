﻿using Android.Util;
using Android.Views;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Essentials;
using System.Linq;

namespace SoftWing.SwSystem
{
    public static class SwSettings
    {
        private static string TAG = "KeymapStorage";
        private static string KEYMAP_SELECTION_FILENAME = "SelectedKeymap.txt";
        private static string KEYMAP_SELECTION_PATH = Path.Combine(FileSystem.AppDataDirectory, KEYMAP_SELECTION_FILENAME);
        private static string KEYMAP_DIRECTORY = Path.Combine(FileSystem.AppDataDirectory, "keymaps");
        private static string OPEN_SOUND_FILENAME = "open_sound.txt";
        private static string CLOSE_SOUND_FILENAME = "close_sound.txt";
        private static string OPEN_SOUND_RECORD_PATH = Path.Combine(FileSystem.AppDataDirectory, OPEN_SOUND_FILENAME);
        private static string CLOSE_SOUND_RECORD_PATH = Path.Combine(FileSystem.AppDataDirectory, CLOSE_SOUND_FILENAME);
        private static string VIBRATION_ENABLE_FILENAME = "vibration_enable.txt";
        private static string VIBRATION_ENABLE_PATH = Path.Combine(FileSystem.AppDataDirectory, VIBRATION_ENABLE_FILENAME);
        private static string TRANSITION_DELAY_FILENAME = "transition_delay.txt";
        private static string TRANSITION_DELAY_PATH = Path.Combine(FileSystem.AppDataDirectory, TRANSITION_DELAY_FILENAME);
        private static string LAYOUT_SELECTION_DIRECTORY = Path.Combine(FileSystem.AppDataDirectory, "layouts");
        private static bool local_keymap_updated = false;
        public static string Default_Keymap_Filename = "Default";
        public const bool Default_Vibration_Enable = true;
        public const int Default_Transition_Delay_ms = 1000;
        public static MotionDescription Default_Motion = MotionDescription.InvalidMotion();
        public static int Default_Layout = Resource.Layout.input_a;
        public enum AnalogDirection : int
        {
            Up, Down, Left, Right
        }
        public static Dictionary<ControlId, Dictionary<AnalogDirection, ControlId>> ANALOG_TO_DIRECTION_MAP = 
            new Dictionary<ControlId, Dictionary<AnalogDirection, ControlId>>
        {
            {
                ControlId.L_Analog,
                new Dictionary<AnalogDirection, ControlId>{
                    { AnalogDirection.Up,    ControlId.L_Analog_Up    },
                    { AnalogDirection.Down,  ControlId.L_Analog_Down  },
                    { AnalogDirection.Left,  ControlId.L_Analog_Left  },
                    { AnalogDirection.Right, ControlId.L_Analog_Right },
                }
            },
            {
                ControlId.R_Analog,
                new Dictionary<AnalogDirection, ControlId>{
                    { AnalogDirection.Up,    ControlId.R_Analog_Up    },
                    { AnalogDirection.Down,  ControlId.R_Analog_Down  },
                    { AnalogDirection.Left,  ControlId.R_Analog_Left  },
                    { AnalogDirection.Right, ControlId.R_Analog_Right },
                }
            },
        };
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
            { ControlId.L1_Button       , "L1 Shoulder Button"   },
            { ControlId.L2_Button       , "L2 Shoulder Button"   },
            { ControlId.R1_Button       , "R1 Shoulder Button"   },
            { ControlId.R2_Button       , "R2 Shoulder Button"   },
            { ControlId.L_Analog        , "Left Analog"          },
            { ControlId.L_Analog_Up     , "Left Analog Up"       },
            { ControlId.L_Analog_Down   , "Left Analog Down"     },
            { ControlId.L_Analog_Left   , "Left Analog Left"     },
            { ControlId.L_Analog_Right  , "Left Analog Right"    },
            { ControlId.R_Analog        , "Right Analog"         },
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
        private static Dictionary<ControlId, SwControl> DEFAULT_CONTROL_MAP = new Dictionary<ControlId, SwControl>
        {
            { ControlId.L1_Button       , new SwControl(ControlId.L1_Button     , key_in: Keycode.ButtonL1)      },
            { ControlId.L2_Button       , new SwControl(ControlId.L2_Button     , key_in: Keycode.ButtonL2)      },
            { ControlId.R1_Button       , new SwControl(ControlId.R1_Button     , key_in: Keycode.ButtonR1)      },
            { ControlId.R2_Button       , new SwControl(ControlId.R2_Button     , key_in: Keycode.ButtonR2)      },
            { ControlId.L_Analog        , new SwControl(ControlId.L_Analog      )                                },
            { ControlId.L_Analog_Up     , new SwControl(ControlId.L_Analog_Up   , key_in: Keycode.W)             },
            { ControlId.L_Analog_Down   , new SwControl(ControlId.L_Analog_Down , key_in: Keycode.S)             },
            { ControlId.L_Analog_Left   , new SwControl(ControlId.L_Analog_Left , key_in: Keycode.A)             },
            { ControlId.L_Analog_Right  , new SwControl(ControlId.L_Analog_Right, key_in: Keycode.D)             },
            { ControlId.R_Analog        , new SwControl(ControlId.R_Analog      )                                },
            { ControlId.R_Analog_Up     , new SwControl(ControlId.R_Analog_Up   , key_in: Keycode.Button1)       },
            { ControlId.R_Analog_Down   , new SwControl(ControlId.R_Analog_Down , key_in: Keycode.Button2)       },
            { ControlId.R_Analog_Left   , new SwControl(ControlId.R_Analog_Left , key_in: Keycode.Button3)       },
            { ControlId.R_Analog_Right  , new SwControl(ControlId.R_Analog_Right, key_in: Keycode.Button4)       },
            { ControlId.X_Button        , new SwControl(ControlId.X_Button      , key_in: Keycode.ButtonX)       },
            { ControlId.Y_Button        , new SwControl(ControlId.Y_Button      , key_in: Keycode.ButtonY)       },
            { ControlId.A_Button        , new SwControl(ControlId.A_Button      , key_in: Keycode.ButtonA)       },
            { ControlId.B_Button        , new SwControl(ControlId.B_Button      , key_in: Keycode.ButtonB)       },
            { ControlId.Start_Button    , new SwControl(ControlId.Start_Button  , key_in: Keycode.ButtonStart)   },
            { ControlId.D_Pad_Up        , new SwControl(ControlId.D_Pad_Up      , key_in: Keycode.DpadUp)        },
            { ControlId.D_Pad_Down      , new SwControl(ControlId.D_Pad_Down    , key_in: Keycode.DpadDown)      },
            { ControlId.D_Pad_Left      , new SwControl(ControlId.D_Pad_Left    , key_in: Keycode.DpadLeft)      },
            { ControlId.D_Pad_Right     , new SwControl(ControlId.D_Pad_Right   , key_in: Keycode.DpadRight)     },
            { ControlId.D_Pad_Center    , new SwControl(ControlId.D_Pad_Center  , key_in: Keycode.DpadCenter)    }
        };
        private static Dictionary<ControlId, SwControl> CONTROL_MAP = new Dictionary<ControlId, SwControl>(DEFAULT_CONTROL_MAP);
        public static Dictionary<int, ControlId> RESOURCE_TO_CONTROL_MAP = new Dictionary<int, ControlId>
        {
            {Resource.Id.l1_button,         ControlId.L1_Button   },
            {Resource.Id.l2_button,         ControlId.L2_Button   },
            {Resource.Id.r1_button,         ControlId.R1_Button   },
            {Resource.Id.r2_button,         ControlId.R2_Button   },
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

        public static int GetTransitionDelayMs()
        {
            Log.Debug(TAG, "GetTransitionDelayMs");
            if (!File.Exists(TRANSITION_DELAY_PATH))
            {
                Log.Debug(TAG, "Delay record not found");
                return Default_Transition_Delay_ms;
            }
            var stream = File.OpenRead(TRANSITION_DELAY_PATH);
            using (var reader = new StreamReader(stream))
            {
                var delayStr = reader.ReadLine().Replace("\n", "").Replace("\r", "");
                if (int.TryParse(delayStr, out int delay))
                {
                    return delay;
                }
                return Default_Transition_Delay_ms;
            }
        }

        public static void SetTransitionDelayMs(int delay_ms)
        {
            Log.Debug(TAG, "SetTransitionDelayMs");
            using (var writer = File.CreateText(TRANSITION_DELAY_PATH))
            {
                writer.WriteLine(delay_ms.ToString());
            }
        }

        public static bool IsAnalogControl(ControlId id)
        {
            return GetAnalogFromDirection(id) != null;
        }

        public static ControlId? GetAnalogFromDirection(ControlId id)
        {
            foreach (var root_control in ANALOG_TO_DIRECTION_MAP.Keys)
            {
                if ((id == root_control) || (ANALOG_TO_DIRECTION_MAP[root_control].Values.Contains(id)))
                {
                    return root_control;
                }
            }
            return null;
        }

        public static List<string> GetKeymapList()
        {
            Log.Debug(TAG, "GetKeymapList");
            if (!Directory.Exists(KEYMAP_DIRECTORY))
            {
                return new List<string> { Default_Keymap_Filename };
            }
            var output = new List<string> { };
            foreach (string file in Directory.GetFiles(KEYMAP_DIRECTORY))
                output.Add(Path.GetFileName(file));
            return output;
        }

        public static string GetSelectedKeymap()
        {
            Log.Debug(TAG, "GetSelectedKeymap");
            if (!File.Exists(KEYMAP_SELECTION_PATH))
            {
                Log.Debug(TAG, "Selected keymap record not found");
                SetSelectedKeymap(Default_Keymap_Filename);
                return Default_Keymap_Filename;
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
            var LAYOUT_SELECTION_PATH = Path.Combine(LAYOUT_SELECTION_DIRECTORY, GetSelectedKeymap());
            if (!File.Exists(LAYOUT_SELECTION_PATH))
            {
                Log.Debug(TAG, "Selected layout record not found");
                SetSelectedLayout(Default_Layout);
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
            var LAYOUT_SELECTION_PATH = Path.Combine(LAYOUT_SELECTION_DIRECTORY, GetSelectedKeymap());
            if (!Directory.Exists(LAYOUT_SELECTION_DIRECTORY))
            {
                Directory.CreateDirectory(LAYOUT_SELECTION_DIRECTORY);
            }
            using (var writer = File.CreateText(LAYOUT_SELECTION_PATH))
            {
                writer.WriteLine(layoutId.ToString());
            }
        }

        public static void SetControlBehavior(ControlId control, ButtonBehavior behavior)
        {
            Log.Debug(TAG, "SetControlBehavior");
            if (IsAnalogControl(control))
            {
                Log.Error(TAG, "Cannot set control behavior for analog");
                return;
            }
            CONTROL_MAP[control].behavior = behavior;
            UpdateStoredKeymap();
        }

        public static ButtonBehavior GetControlBehavior(ControlId control)
        {
            Log.Debug(TAG, "GetControlBehavior");
            UpdateLocalKeymap();
            return CONTROL_MAP[control].behavior;
        }

        public static void SetControlKeycode(ControlId control, Keycode key)
        {
            Log.Debug(TAG, "SetControlKeycode");
            if (IsAnalogControl(control))
            {
                CONTROL_MAP[(ControlId)GetAnalogFromDirection(control)].motion = MotionDescription.InvalidMotion();
            }
            CONTROL_MAP[control].key = key;
            CONTROL_MAP[control].motion = MotionDescription.InvalidMotion();
            UpdateStoredKeymap();
        }

        public static Keycode GetControlKeycode(ControlId control)
        {
            Log.Debug(TAG, "GetControlKeycode");
            UpdateLocalKeymap();
            return CONTROL_MAP[control].key;
        }

        public static void SetControlMotion(ControlId control, MotionDescription motion)
        {
            Log.Debug(TAG, "SetControlMotion");
            if (IsAnalogControl(control))
            {
                control = (ControlId)GetAnalogFromDirection(control);
                foreach (var dir_control in ANALOG_TO_DIRECTION_MAP[control].Values)
                {
                    CONTROL_MAP[dir_control].key = Keycode.Unknown;
                }
            }
            CONTROL_MAP[control].motion = motion;
            CONTROL_MAP[control].key = Keycode.Unknown;
            UpdateStoredKeymap();
        }

        public static MotionDescription GetControlMotion(ControlId control)
        {
            Log.Debug(TAG, "GetControlMotion");
            UpdateLocalKeymap();
            if (IsAnalogControl(control))
            {
                control = (ControlId)GetAnalogFromDirection(control);
            }
            return CONTROL_MAP[control].motion;
        }

        private static void ResetLocalKeymap()
        {
            Log.Debug(TAG, "ResetLocalKeymap");
            var control_keys = CONTROL_TO_STRING_MAP.Keys;
            CONTROL_MAP = new Dictionary<ControlId, SwControl>(DEFAULT_CONTROL_MAP);
        }

        public static void DeleteStoredKeymap(string name)
        {
            var keymapPath = Path.Combine(KEYMAP_DIRECTORY, name);
            File.Delete(keymapPath);
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
            var keymapPath = Path.Combine(KEYMAP_DIRECTORY, keymapName);
            if (!File.Exists(keymapPath))
            {
                Log.Debug(TAG, "Keymap not found: " + keymapPath);
                UpdateStoredKeymap();
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
                        Log.Debug(TAG, "\t" + line);
                        var new_control = SwControl.Deserialize(line);
                        CONTROL_MAP[new_control.id] = new_control;
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
            var keymapName = GetSelectedKeymap();
            var keymapPath = Path.Combine(KEYMAP_DIRECTORY, keymapName);
            Log.Debug(TAG, "UpdateStoredKeymap: " + keymapPath);
            using (var writer = File.CreateText(keymapPath))
            {
                foreach (ControlId control in Enum.GetValues(typeof(ControlId)))
                {
                    if (control == ControlId.Unknown)
                    {
                        continue;
                    }
                    var writetext = CONTROL_MAP[control].Serialize();
                    Log.Debug(TAG, "\t" + writetext);
                    writer.WriteLine(writetext);
                }
            }
        }

        public static string KeycodeToString(Keycode key)
        {
            return Enum.GetName(typeof(Keycode), key);
        }

        public static Keycode StringToKeycode(string key_string)
        {
            foreach (Keycode key_code in Enum.GetValues(typeof(Keycode)))
            {
                if (key_string == KeycodeToString(key_code))
                {
                    return key_code;
                }
            }
            return Keycode.Unknown;
        }

        public static string ButtonBehaviorToString(ButtonBehavior behavior)
        {
            return Enum.GetName(typeof(ButtonBehavior), behavior);
        }

        public static ButtonBehavior StringToButtonBehavior(string behavior_string)
        {
            foreach (ButtonBehavior behavior in Enum.GetValues(typeof(ButtonBehavior)))
            {
                if (behavior_string == ButtonBehaviorToString(behavior))
                {
                    return behavior;
                }
            }
            return ButtonBehavior.Temporary;
        }
    }
}
