﻿using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Windows.Forms;

using IVSDKDotNet;
using IVSDKDotNet.Attributes;
using IVSDKDotNet.Enums;

using Manager.Classes;
using Manager.Classes.Json.Script;

namespace Manager.UI
{
    internal class ManagerUI
    {

        #region Variables
        public static bool IsConfigUIOpened;
        private static bool invalidKey;

        private static int selectedScriptIndex;
        private static DateTime nextRefresh = DateTime.MinValue;

        private static double initializedTS;
        private static double tickTS;
        private static double gameLoadTS;
        private static double gameLoadPriorityTS;
        private static double mountDeviceTS;
        private static double drawingTS;
        private static double processCamTS;
        private static double processAutoTS;
        private static double processPadTS;
        private static double firstD3D9FrameTS;
        private static double keyDownTS;
        private static double keyUpTS;
        private static double ingameStartupTS;
        #endregion

        #region Methods
        private static void RefreshExecutionTimes(Script script)
        {
            // Refresh times
            if (DateTime.UtcNow > nextRefresh)
            {
                initializedTS = script.InitializedEventExecutionTime.TotalSeconds;
                tickTS = script.TickEventExecutionTime.TotalSeconds;
                gameLoadTS = script.GameLoadEventExecutionTime.TotalSeconds;
                gameLoadPriorityTS = script.GameLoadPriorityEventExecutionTime.TotalSeconds;
                mountDeviceTS = script.MountDeviceEventExecutionTime.TotalSeconds;
                drawingTS = script.DrawingEventExecutionTime.TotalSeconds;
                processCamTS = script.ProcessCameraEventExecutionTime.TotalSeconds;
                processAutoTS = script.ProcessAutomobileEventExecutionTime.TotalSeconds;
                processPadTS = script.ProcessPadEventExecutionTime.TotalSeconds;
                firstD3D9FrameTS = script.OnFirstD3D9FrameEventExecutionTime.TotalSeconds;
                keyDownTS = script.KeyDownEventExecutionTime.TotalSeconds;
                keyUpTS = script.KeyUpEventExecutionTime.TotalSeconds;
                ingameStartupTS = script.IngameStartupEventExecutionTime.TotalSeconds;

                // Set next refresh time
                nextRefresh = DateTime.UtcNow.AddSeconds(1d);
            }

            ImGuiIV.Text("Initialized: {0}s", initializedTS);
            ImGuiIV.Text("Tick: {0}s", tickTS);
            ImGuiIV.Text("GameLoad: {0}s", gameLoadTS);
            ImGuiIV.Text("GameLoadPriority: {0}s", gameLoadPriorityTS);
            ImGuiIV.Text("MountDevice: {0}s", mountDeviceTS);
            ImGuiIV.Text("Drawing: {0}s", drawingTS);
            ImGuiIV.Text("ProcessCamera: {0}s", processCamTS);
            ImGuiIV.Text("ProcessAutomobile: {0}s", processAutoTS);
            ImGuiIV.Text("ProcessPad: {0}s", processPadTS);
            ImGuiIV.Text("OnFirstD3D9Frame: {0}s", firstD3D9FrameTS);
            ImGuiIV.Text("KeyDown: {0}s", keyDownTS);
            ImGuiIV.Text("KeyUp: {0}s", keyUpTS);
            ImGuiIV.Text("IngameStartup: {0}s", ingameStartupTS);
        }
        #endregion

        #region Functions
        private static bool GetAttribute<T>(FieldInfo field, out T attribute)
        {
            object foundAttribute = field.GetCustomAttribute(typeof(T));

            if (foundAttribute == null)
            {
                attribute = default(T);
                return false;
            }

            attribute = (T)foundAttribute;
            return true;
        }
        #endregion

        public static void DoUI()
        {
            if (!IsConfigUIOpened)
                return;

            Main main = Main.Instance;

            Vector2 mousePos = ImGuiIV.GetMousePos();

            ImGuiIV.Begin("IV-SDK .NET Manager", ref IsConfigUIOpened, eImGuiWindowFlags.None);
            ImGuiIV.SetWindowSize(new Vector2(450f, 500f), eImGuiCond.Once);

            if (ImGuiIV.BeginTabBar("##MainTabBar"))
            {
                //ImGuiIV.SetWindowSize(new Vector2(300f, 400f), eImGuiCond.Once);

#if DEBUG
                // Debug
                if (ImGuiIV.BeginTabItem("Debug"))
                {
                    // ScriptHookDotNet
                    ImGuiIV.Spacing();
                    ImGuiIV.SeparatorText("ScriptHookDotNet");
                    ImGuiIV.CheckBox("Enable Verbose Logging", ref SHDNStuff.EnableVerboseLogging);
                    ImGuiIV.BeginDisabled();
                    ImGuiIV.CheckBox("WereScriptHookDotNetScriptsLoadedThisSession", ref SHDNStuff.WereScriptHookDotNetScriptsLoadedThisSession);
                    ImGuiIV.EndDisabled();

                    ImGuiIV.Text("CurrentConstructingScript: {0}", SHDNStuff.CurrentConstructingScript != null ? SHDNStuff.CurrentConstructingScript.GUID : Guid.Empty);
                    ImGuiIV.Text("CurrentTickScript: {0}", SHDNStuff.CurrentTickScript != null ? SHDNStuff.CurrentTickScript.GUID : Guid.Empty);
                    ImGuiIV.Text("CurrentMouseDownScript: {0}", SHDNStuff.CurrentMouseDownScript != null ? SHDNStuff.CurrentMouseDownScript.GUID : Guid.Empty);
                    ImGuiIV.Text("CurrentMouseUpScript: {0}", SHDNStuff.CurrentMouseUpScript != null ? SHDNStuff.CurrentMouseUpScript.GUID : Guid.Empty);
                    ImGuiIV.Text("CurrentScriptCommandScript: {0}", SHDNStuff.CurrentScriptCommandScript != null ? SHDNStuff.CurrentScriptCommandScript.GUID : Guid.Empty);
                    ImGuiIV.Text("CurrentPerFrameDrawingScript: {0}", SHDNStuff.CurrentPerFrameDrawingScript != null ? SHDNStuff.CurrentPerFrameDrawingScript.GUID : Guid.Empty);

                    // Lists
                    ImGuiIV.Spacing();
                    ImGuiIV.SeparatorText("Lists");
                    ImGuiIV.Text("Local Tasks: {0}", main.LocalTasks.Count);
                    ImGuiIV.Text("Delayed Actions: {0}", main.DelayedActions.Count);

                    // States
                    ImGuiIV.Spacing();
                    ImGuiIV.SeparatorText("States");
                    ImGuiIV.BeginDisabled();
                    ImGuiIV.CheckBox("FirstFrame", ref main.FirstFrame);
                    ImGuiIV.CheckBox("IsGTAIVWindowInFocus", ref main.IsGTAIVWindowInFocus);
                    ImGuiIV.CheckBox("OnWindowFocusChangedEventCalled", ref main.OnWindowFocusChangedEventCalled);
                    ImGuiIV.CheckBox("WasBoundPhoneNumbersProcessed", ref main.WasBoundPhoneNumbersProcessed);
                    ImGuiIV.EndDisabled();

                    // Player
                    ImGuiIV.Spacing();
                    ImGuiIV.SeparatorText("Player");
                    ImGuiIV.Text("Player Ped Pointer: {0}", IVPlayerInfo.FindThePlayerPed());

                    // Phone
                    IVPhoneInfo thePhoneInfo = IVPhoneInfo.ThePhoneInfo;
                    if (thePhoneInfo != null && IVPlayerInfo.FindThePlayerPed() != UIntPtr.Zero)
                    {
                        ImGuiIV.Spacing();
                        ImGuiIV.SeparatorText("Phone");
                        ImGuiIV.Text("Current Phone Number: {0}", thePhoneInfo.CurrentNumberInput);
                        ImGuiIV.Text("Phone State: {0}", (ePhoneState)thePhoneInfo.State);
                    }

                    ImGuiIV.EndTabItem();
                }
#endif

                // Settings
                if (ImGuiIV.BeginTabItem("Settings"))
                {
                    ImGuiIV.Text("See and change some IV-SDK .NET settings.");
                    ImGuiIV.Text("This page is build up like the config.ini file structure.");
                    ImGuiIV.Dummy(new Vector2(0f, 3f));

                    if (ImGuiIV.Button("Save settings"))
                        Config.SaveSettings();
                    ImGuiIV.SameLine();
                    if (ImGuiIV.Button("Restore defaults"))
                        ImGuiIV.OpenPopup("Restore default settings?");

                    ImGuiIV.CreateSimplePopupDialog("Restore default settings?", string.Format("This will reset all settings to its default value and will save the file.{0}" +
                        "Are you sure you want to restore the default settings?", Environment.NewLine), true, true, true, "Yes", "No", () =>
                    {
                        Config.RestoreDefaults();
                        Config.SaveSettings();
                    }, () => ImGuiIV.CloseCurrentPopup());

                    ImGuiIV.Dummy(new Vector2(0f, 3f));

                    if (ImGuiIV.TreeNode("Main"))
                    {

                        ImGuiIV.HelpMarker(string.Format("Sets if the IVSDKDotNet.log file should be created in the main directory of GTA IV or not.{0}" +
                            "If set to false, the Log files will be created in the 'logs' folder within the 'IVSDKDotNet' directory.", Environment.NewLine));
                        ImGuiIV.SameLine();
                        ImGuiIV.CheckBox("Create Log Files In Main Directory", ref Config.CreateLogFilesInMainDirectory);

                        ImGuiIV.HelpMarker("Determines how many logs files can be stored inside the 'logs' folder within the 'IVSDKDotNet' directory.");
                        ImGuiIV.SameLine();
                        ImGuiIV.DragInt("Max Logs Files", ref Config.MaxLogsFiles);

                        ImGuiIV.HelpMarker(string.Format("Defines the key that is used to switch the cursor visiblity which is used for script windows, the IV-SDK .NET console and manager window.{0}" +
                            "If the cursor is visible, the mouse input of GTA IV will be disabled.", Environment.NewLine));
                        ImGuiIV.SameLine();
                        ImGuiIV.InputText("Switch Cursor Key", ref Config.SwitchCursorKey);

                        ImGuiIV.HelpMarker(string.Format("Defines the key that is used to open/close the IV-SDK .NET manager window.", Environment.NewLine));
                        ImGuiIV.SameLine();
                        ImGuiIV.InputText("Open Manager Window Key", ref Config.OpenManagerWindowKey);

                        ImGuiIV.TreePop();
                    }
                    if (ImGuiIV.TreeNode("Scripts"))
                    {

                        ImGuiIV.HelpMarker("Sets if all running scripts will pause from executing when the GTA IV window is not currently in focus.");
                        ImGuiIV.SameLine();
                        ImGuiIV.CheckBox("Pause Execution When Not In Focus", ref Config.PauseScriptExecutionWhenNotInFocus);

                        ImGuiIV.HelpMarker(string.Format("This prevents scripts from loading if the IVSDKDotNetWrapper version the script was made with is older then the current one.{0}" +
                            "Setting this to false will load every script even if the IVSDKDotNetWrapper version the script was made with is older then the current one.", Environment.NewLine));
                        ImGuiIV.SameLine();
                        ImGuiIV.CheckBox("Do Not Load Legacy Scripts", ref Config.DoNotLoadLegacyScripts);

                        ImGuiIV.HelpMarker("This reloads all scripts right before loading a save, starting a new game or when switching episodes.");
                        ImGuiIV.SameLine();
                        ImGuiIV.CheckBox("Reload Scripts On Reload", ref Config.ReloadScriptsOnReload);

                        ImGuiIV.HelpMarker("This will load your ScriptHookDotNet mods within the 'scripts' folder located in the main directory of GTA IV.");
                        ImGuiIV.SameLine();
                        ImGuiIV.CheckBox("Load ScriptHookDotNet Scripts", ref Config.LoadScriptHookDotNetScripts);

                        ImGuiIV.TreePop();
                    }
                    if (ImGuiIV.TreeNode("Notification"))
                    {

                        ImGuiIV.HelpMarker("If set to false, no notifications will be shown on screen. This is not recommended as you might miss important notifications.");
                        ImGuiIV.SameLine();
                        ImGuiIV.CheckBox("Show Notifications", ref Config.ShowNotifications);

                        ImGuiIV.TreePop();
                    }
                    if (ImGuiIV.TreeNode("Console"))
                    {

                        string key = Config.ConsoleOpenCloseKey.ToString();

                        if (invalidKey)
                            ImGuiIV.PushStyleColor(eImGuiCol.ImGuiCol_Text, Color.Red);

                        ImGuiIV.HelpMarker("Defines the key that is used to open/close the IV-SDK .NET console.");
                        ImGuiIV.SameLine();
                        ImGuiIV.InputText("Open/Close Key", ref key);

                        if (invalidKey)
                            ImGuiIV.PopStyleColor();

                        if (Enum.TryParse<Keys>(key, out Keys keys))
                        {
                            Config.ConsoleOpenCloseKey = keys;
                            invalidKey = false;
                        }
                        else
                        {
                            invalidKey = true;
                        }

                        ImGuiIV.TreePop();
                    }
                    if (ImGuiIV.TreeNode("API"))
                    {

                        ImGuiIV.HelpMarker(string.Format("Sets if API Connections are allowed.{0}" +
                            "If set to true, applications can connect to the IV-SDK .NET Manager via the API and see, for example, which scripts are running, or can even reload them.", Environment.NewLine));
                        ImGuiIV.SameLine();
                        if (ImGuiIV.CheckBox("Allow Connections", ref Config.AllowRemoteConnections))
                        {
                            if (Config.AllowRemoteConnections)
                                Main.Instance.ConnectionManager.Start(false);
                        }

                        ImGuiIV.HelpMarker("Shows a notification when some application connected with the IV-SDK .NET Manager via the API.");
                        ImGuiIV.SameLine();
                        ImGuiIV.CheckBox("Show Notification On Connection", ref Config.ShowNotificationOnConnection);

                        ImGuiIV.HelpMarker("Allows connected API clients to reload scripts.");
                        ImGuiIV.SameLine();
                        ImGuiIV.CheckBox("Allow Remote Reload Scripts", ref Config.AllowRemoteReloadScripts);

                        ImGuiIV.HelpMarker("Allows connected API clients to load scripts.");
                        ImGuiIV.SameLine();
                        ImGuiIV.CheckBox("Allow Remote Load Scripts", ref Config.AllowRemoteLoadScripts);

                        ImGuiIV.HelpMarker("Allows connected API clients to abort scripts.");
                        ImGuiIV.SameLine();
                        ImGuiIV.CheckBox("Allow Remote Abort Scripts", ref Config.AllowRemoteAbortScripts);

                        ImGuiIV.HelpMarker(string.Format("Allows connected API clients to call GTA IV Native Functions like 'ADD_EXPLOSION' or 'SET_CHAR_COORDINATES'.{0}" +
                            "As you might can tell, this could be abused in some way. Turn this off to avoid other applications being able to call those Functions.", Environment.NewLine));
                        ImGuiIV.SameLine();
                        ImGuiIV.CheckBox("Allow Remote Native Function Calls", ref Config.AllowRemoteNativeFunctionCalls);

                        ImGuiIV.TreePop();
                    }

                    ImGuiIV.EndTabItem();
                }

                // Scripts
                if (ImGuiIV.BeginTabItem("Scripts"))
                {
                    ImGuiIV.SeparatorText("Control");

                    // Time since last script reload
                    TimeSpan timeSinceLastScriptReload = DateTime.Now - main.TimeSinceLastScriptReload;

                    if (timeSinceLastScriptReload.TotalSeconds < 59d)
                        ImGuiIV.Text("Last Script Reload Occured At: {0} ({1} second(s) ago)", main.TimeSinceLastScriptReload, (int)timeSinceLastScriptReload.TotalSeconds);
                    else if (timeSinceLastScriptReload.TotalMinutes < 61d)
                        ImGuiIV.Text("Last Script Reload Occured At: {0} ({1} minute(s) ago)", main.TimeSinceLastScriptReload, (int)timeSinceLastScriptReload.TotalMinutes);
                    else
                        ImGuiIV.Text("Last Script Reload Occured At: {0} ({1} hour(s) ago)", main.TimeSinceLastScriptReload, (int)timeSinceLastScriptReload.TotalHours);

                    if (ImGuiIV.Button("Abort scripts"))
                        main.AbortScripts(ScriptType.All, AbortReason.Manager, false);
                    ImGuiIV.SameLine();
                    if (ImGuiIV.Button("Reload scripts"))
                        main.LoadScripts();

                    ImGuiIV.Text("Active IV-SDK .NET Scripts: {0}", main.ActiveScripts.Count(x => x.IsIVSDKDotNetScript));
                    ImGuiIV.Text("Active ScriptHookDotNet Scripts: {0}", main.ActiveScripts.Count(x => x.IsScriptHookDotNetScript));

                    ImGuiIV.Spacing();
                    ImGuiIV.SeparatorText("Scripts");

                    if (main.ActiveScripts.Count != 0)
                    {
                        ImGuiIV.Text("Select script to see and edit its information");

                        // Set the selected index to 0 if it is bigger then the currently loaded scripts count 
                        if (selectedScriptIndex > (main.ActiveScripts.Count - 1))
                            selectedScriptIndex = 0;

                        // Select Script
                        ImGuiIV.Text("Total active scripts: {0}", main.ActiveScripts.Count);
                        if (ImGuiIV.BeginCombo("##ScriptsComboBox", main.ActiveScripts[selectedScriptIndex].Name))
                        {
                            for (int i = 0; i < main.ActiveScripts.Count; i++)
                            {
                                bool isSelected = selectedScriptIndex == i;

                                if (ImGuiIV.Selectable(main.ActiveScripts[i].Name, isSelected))
                                    selectedScriptIndex = i;

                                if (isSelected)
                                    ImGuiIV.SetItemDefaultFocus();
                            }
                            ImGuiIV.EndCombo();
                        }

                        // Get script from index
                        FoundScript foundScript = main.ActiveScripts[selectedScriptIndex];
                        bool isIVSDKDotNetScript = foundScript.IsIVSDKDotNetScript;

                        ImGuiIV.Spacing();
                        ImGuiIV.SeparatorText(foundScript.Name);

                        if (ImGuiIV.Button("Abort"))
                        {
                            main.AbortScriptInternal(AbortReason.Manager, foundScript, false);
                            ImGuiIV.EndTabItem();
                            ImGuiIV.EndTabBar();
                            ImGuiIV.End();
                            return;
                        }
                        ImGuiIV.SameLine();
                        if (ImGuiIV.Button(foundScript.Running ? "Pause script" : "Resume script"))
                            foundScript.Running = !foundScript.Running;

                        ImGuiIV.Dummy(new Vector2(0f, 10f));
                        if (ImGuiIV.BeginTabBar("##ScriptTabBar"))
                        {

                            // Script Details
                            if (ImGuiIV.BeginTabItem("Details"))
                            {

                                if (isIVSDKDotNetScript)
                                    ImGuiIV.Text("Type: IVSDKDotNet Script");
                                else
                                    ImGuiIV.Text("Type: ScriptHookDotNet Script");

                                ImGuiIV.Text("ID: {0}", foundScript.ID);
                                ImGuiIV.Text("Full Path: {0}", foundScript.FullPath);
                                ImGuiIV.SetItemTooltip(foundScript.FullPath);
                                ImGuiIV.Text("Is Script Ready: {0}", foundScript.IsScriptReady());

                                ImGuiIV.BeginDisabled();
                                ImGuiIV.CheckBox("Running", ref foundScript.Running);
                                if (isIVSDKDotNetScript)
                                    ImGuiIV.CheckBox("Init Event Called", ref foundScript.InitEventCalled);
                                ImGuiIV.EndDisabled();

                                // Script Configuration
                                if (isIVSDKDotNetScript)
                                {
                                    ImGuiIV.SeparatorText("Script Configuration");

                                    if (foundScript.Config == null)
                                    {
                                        ImGuiIV.Text("This script has no configuration file.");
                                    }
                                    else
                                    {
                                        if (!foundScript.Config.HasDependencyInfo())
                                        {
                                            ImGuiIV.Text("This script has no dependency info.");
                                        }
                                        else
                                        {
                                            if (ImGuiIV.TreeNode("View Dependency Info"))
                                            {

                                                for (int i = 0; i < foundScript.Config.DependencyInfo.Count; i++)
                                                {
                                                    ScriptDependencyInfo dependencyInfo = foundScript.Config.DependencyInfo[i];

                                                    ImGuiIV.SeparatorText(dependencyInfo.Name + " (.dll)");

                                                    ImGuiIV.HelpMarker("Where this dependency can be downloaded.");
                                                    ImGuiIV.SameLine();
                                                    ImGuiIV.Text("DownloadURL: {0}", dependencyInfo.DownloadURL);
                                                    ImGuiIV.SetItemTooltip(dependencyInfo.DownloadURL);

                                                    ImGuiIV.HelpMarker("The ID of this dependency in the IV Launcher Workshop.");
                                                    ImGuiIV.SameLine();
                                                    ImGuiIV.Text("IVLauncherWorkshopID: {0}", dependencyInfo.IVLauncherWorkshopID.ToString());
                                                    ImGuiIV.SetItemTooltip(dependencyInfo.IVLauncherWorkshopID.ToString());

                                                    if (i != 0)
                                                        ImGuiIV.Separator();
                                                }

                                                ImGuiIV.Spacing();
                                                ImGuiIV.TreePop();
                                            }
                                        }

                                        if (!foundScript.Config.HasIncompatibleMods())
                                        {
                                            ImGuiIV.Text("This script has no incompatible mods info.");
                                        }
                                        else
                                        {
                                            if (ImGuiIV.TreeNode("View Incompatible Mods"))
                                            {

                                                for (int i = 0; i < foundScript.Config.IncompatibleMods.Count; i++)
                                                {
                                                    ImGuiIV.Text(foundScript.Config.IncompatibleMods[i]);
                                                }

                                                ImGuiIV.Spacing();
                                                ImGuiIV.TreePop();
                                            }
                                        }
                                    }

                                    ImGuiIV.Spacing();
                                }

                                // Script Tasks
                                ImGuiIV.SeparatorText("Script Tasks");
                                ImGuiIV.Text("Active Script Tasks: {0}", foundScript.ScriptTasks.Count);
                                ImGuiIV.Spacing();

                                // Console Commands
                                ImGuiIV.SeparatorText("Console Commands");

                                if (foundScript.ConsoleCommands.Count == 0)
                                {
                                    ImGuiIV.Text("This script has no console commands registered.");
                                }
                                else
                                {
                                    if (ImGuiIV.TreeNode(string.Format("Registered Console Commands: {0}", foundScript.ConsoleCommands.Count)))
                                    {

                                        ImGuiIV.BeginListBox("##ScriptConsoleCommands", new Vector2(ImGuiIV.FloatMin, 40f));

                                        string[] commands = foundScript.ConsoleCommands.Keys.ToArray();

                                        for (int c = 0; c < commands.Length; c++)
                                            ImGuiIV.Text(commands[c]);

                                        ImGuiIV.EndListBox();

                                        ImGuiIV.TreePop();
                                    }
                                }

                                ImGuiIV.Spacing();

                                // Bound Phone Numbers
                                if (isIVSDKDotNetScript)
                                {
                                    ImGuiIV.SeparatorText("Bound Phone Numbers");

                                    if (foundScript.BoundPhoneNumbers.Count == 0)
                                    {
                                        ImGuiIV.Text("This script has no registered phone numbers.");
                                    }
                                    else
                                    {
                                        if (ImGuiIV.TreeNode(string.Format("Registered Phone Numbers: {0}", foundScript.BoundPhoneNumbers.Count)))
                                        {

                                            ImGuiIV.BeginListBox("##ScriptRegisteredPhoneNumbers", new Vector2(ImGuiIV.FloatMin, 40f));

                                            BoundPhoneNumber[] arr = foundScript.BoundPhoneNumbers.Values.ToArray();
                                            for (int c = 0; c < arr.Length; c++)
                                                ImGuiIV.Text(arr[c].Number);

                                            ImGuiIV.EndListBox();

                                            ImGuiIV.TreePop();
                                        }
                                    }

                                    ImGuiIV.Spacing();
                                }

                                // Direct3D9
                                ImGuiIV.SeparatorText("Direct3D9");

                                if (foundScript.Textures.Count == 0)
                                {
                                    ImGuiIV.Text("This script has no registered textures.");
                                }
                                else
                                {
                                    if (ImGuiIV.TreeNode(string.Format("Registered Textures: {0}", foundScript.Textures.Count)))
                                    {
                                        Vector2 button_sz = new Vector2(64f);
                                        float window_visible_x2 = ImGuiIV.GetWindowPos().Y + ImGuiIV.GetWindowContentRegionMax().X * 2f;

                                        for (int i = 0; i < foundScript.Textures.Count; i++)
                                        {
                                            IntPtr txtPtr = foundScript.Textures[i];

                                            ImGuiIV.PushID(i);

                                            Vector2 pos = ImGuiIV.GetCursorScreenPos();
                                            ImGuiIV.Image(txtPtr, button_sz);
                                            ImGuiIV.ImagePreviewToolTip(txtPtr, button_sz, pos, 32f, 5f);

                                            float last_button_x2 = ImGuiIV.GetItemRectMax().X;
                                            float next_button_x2 = last_button_x2 + 10f + button_sz.X; // Expected position if next button was on same line
                                            if (i + 1 < foundScript.Textures.Count && next_button_x2 < window_visible_x2)
                                                ImGuiIV.SameLine();

                                            ImGuiIV.PopID();
                                        }

                                        ImGuiIV.TreePop();
                                    }
                                }

                                ImGuiIV.Spacing();

                                // Execution Times
                                if (isIVSDKDotNetScript)
                                {
                                    ImGuiIV.HelpMarker("Shows you how long it took to execute each event in the script in seconds.");
                                    ImGuiIV.SameLine();
                                    ImGuiIV.SeparatorText("Execution Times");

                                    RefreshExecutionTimes(foundScript.GetScriptAs<Script>());
                                }

                                ImGuiIV.EndTabItem();
                            }

                            // Script Public Fields
                            if (ImGuiIV.BeginTabItem("Public Fields"))
                            {
                                // Set tab item tooltip
                                ImGuiIV.SetItemTooltip("If the script has any fields that are public, they can be manipulated here.\nThink of this as the Unity Inspector.");

                                if (foundScript.PublicFields.Length != 0)
                                {
                                    ImGuiIV.BeginChild("PublicFieldsChild");

                                    for (int f = 0; f < foundScript.PublicFields.Length; f++)
                                    {
                                        FieldInfo info = foundScript.PublicFields[f];

                                        // Early handle custom attributes for members
                                        if (info.GetCustomAttribute<HideInInspectorAttribute>() != null)
                                            continue;

                                        object[] customAttributes = info.GetCustomAttributes(false);
                                        for (int i = 0; i < customAttributes.Length; i++)
                                        {
                                            object attributeObj = customAttributes[i];

                                            if (attributeObj.GetType() == typeof(SeparatorAttribute))
                                            {
                                                SeparatorAttribute a = attributeObj as SeparatorAttribute;

                                                if (string.IsNullOrWhiteSpace(a.Text))
                                                    ImGuiIV.Separator();
                                                else
                                                    ImGuiIV.SeparatorText(a.Text);
                                            }
                                            if (attributeObj.GetType() == typeof(SpaceAttribute))
                                            {
                                                ImGuiIV.Dummy(new Vector2(0f, (attributeObj as SpaceAttribute).Height));
                                            }
                                        }

                                        // byte
                                        if (info.FieldType == typeof(byte))
                                        {
                                            int value = Convert.ToInt32(info.GetValue(foundScript.TheScriptObject));

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.DragInt(string.Format("##{0}", info.Name), ref value))
                                                info.SetValue(foundScript.TheScriptObject, Convert.ToByte(value));
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        }

                                        // sbyte
                                        if (info.FieldType == typeof(sbyte))
                                        {
                                            int value = Convert.ToInt32(info.GetValue(foundScript.TheScriptObject));

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.DragInt(string.Format("##{0}", info.Name), ref value))
                                                info.SetValue(foundScript.TheScriptObject, Convert.ToSByte(value));
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        }

                                        // short
                                        if (info.FieldType == typeof(short))
                                        {
                                            int value = Convert.ToInt32(info.GetValue(foundScript.TheScriptObject));

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.DragInt(string.Format("##{0}", info.Name), ref value))
                                                info.SetValue(foundScript.TheScriptObject, Convert.ToInt16(value));
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        }

                                        // ushort
                                        if (info.FieldType == typeof(ushort))
                                        {
                                            int value = Convert.ToInt32(info.GetValue(foundScript.TheScriptObject));

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.DragInt(string.Format("##{0}", info.Name), ref value))
                                                info.SetValue(foundScript.TheScriptObject, Convert.ToUInt16(value));
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        }

                                        // int
                                        if (info.FieldType == typeof(int))
                                        {
                                            int value = Convert.ToInt32(info.GetValue(foundScript.TheScriptObject));

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.DragInt(string.Format("##{0}", info.Name), ref value))
                                                info.SetValue(foundScript.TheScriptObject, value);
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        }

                                        // uint
                                        if (info.FieldType == typeof(uint))
                                        {
                                            int value = Convert.ToInt32(info.GetValue(foundScript.TheScriptObject));

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.DragInt(string.Format("##{0}", info.Name), ref value))
                                                info.SetValue(foundScript.TheScriptObject, Convert.ToUInt32(value));
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        }

                                        // long
                                        if (info.FieldType == typeof(long))
                                        {
                                            int value = Convert.ToInt32(info.GetValue(foundScript.TheScriptObject));

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.DragInt(string.Format("##{0}", info.Name), ref value))
                                                info.SetValue(foundScript.TheScriptObject, Convert.ToInt64(value));
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        }

                                        // ulong
                                        if (info.FieldType == typeof(ulong))
                                        {
                                            int value = Convert.ToInt32(info.GetValue(foundScript.TheScriptObject));

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.DragInt(string.Format("##{0}", info.Name), ref value))
                                                info.SetValue(foundScript.TheScriptObject, Convert.ToUInt64(value));
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        }

                                        // float
                                        if (info.FieldType == typeof(float))
                                        {
                                            float value = Convert.ToSingle(info.GetValue(foundScript.TheScriptObject));

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.DragFloat(string.Format("##{0}", info.Name), ref value))
                                                info.SetValue(foundScript.TheScriptObject, Convert.ToSingle(value));
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0} (float).", info.FieldType.Name));
                                        }

                                        // double
                                        if (info.FieldType == typeof(double))
                                        {
                                            float value = Convert.ToSingle(info.GetValue(foundScript.TheScriptObject));

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.DragFloat(string.Format("##{0}", info.Name), ref value))
                                                info.SetValue(foundScript.TheScriptObject, Convert.ToDouble(value));
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        }

                                        // decimal
                                        if (info.FieldType == typeof(decimal))
                                        {
                                            float value = Convert.ToSingle(info.GetValue(foundScript.TheScriptObject));

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.DragFloat(string.Format("##{0}", info.Name), ref value))
                                                info.SetValue(foundScript.TheScriptObject, Convert.ToDecimal(value));
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        }

                                        // bool
                                        if (info.FieldType == typeof(bool))
                                        {
                                            bool value = Convert.ToBoolean(info.GetValue(foundScript.TheScriptObject));

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.CheckBox(string.Format("##{0}", info.Name), ref value))
                                                info.SetValue(foundScript.TheScriptObject, value);
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        }

                                        // string
                                        if (info.FieldType == typeof(string))
                                        {
                                            string value = Convert.ToString(info.GetValue(foundScript.TheScriptObject));

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.InputText(string.Format("##{0}", info.Name), ref value))
                                                info.SetValue(foundScript.TheScriptObject, value);
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        }

                                        // Vector2
                                        if (info.FieldType == typeof(Vector2))
                                        {
                                            Vector2 value = (Vector2)info.GetValue(foundScript.TheScriptObject);
                                            float[] arr = new float[2] { value.X, value.Y };

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.DragFloat2(string.Format("##{0}", info.Name), ref arr))
                                                info.SetValue(foundScript.TheScriptObject, new Vector2(arr[0], arr[1]));
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        }

                                        // Vector3
                                        if (info.FieldType == typeof(Vector3))
                                        {
                                            Vector3 value = (Vector3)info.GetValue(foundScript.TheScriptObject);
                                            float[] arr = new float[3] { value.X, value.Y, value.Z };

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.DragFloat3(string.Format("##{0}", info.Name), ref arr))
                                                info.SetValue(foundScript.TheScriptObject, new Vector3(arr[0], arr[1], arr[2]));
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        }

                                        // Quaternion
                                        if (info.FieldType == typeof(Quaternion))
                                        {
                                            Quaternion value = (Quaternion)info.GetValue(foundScript.TheScriptObject);
                                            float[] arr = new float[4] { value.X, value.Y, value.Z, value.W };

                                            ImGuiIV.Text(info.Name);

                                            // Set tooltip if attribute was set
                                            if (GetAttribute(info, out TooltipAttribute attr))
                                                ImGuiIV.SetItemTooltip(attr.Text);

                                            ImGuiIV.SameLine(0f, 30f);
                                            if (ImGuiIV.DragFloat4(string.Format("##{0}", info.Name), ref arr))
                                                info.SetValue(foundScript.TheScriptObject, new Quaternion(arr[0], arr[1], arr[2], arr[3]));
                                            ImGuiIV.SameLine();
                                            ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        }

                                        // Color
                                        //if (info.FieldType == typeof(Color))
                                        //{
                                        //    Color value = (Color)info.GetValue(script.TheScript);
                                        //    Vector4 arr = new Vector4(value.R, value.G, value.B, value.A);

                                        //    ImGuiIV.Text(info.Name);
                                        //    ImGuiIV.SameLine(0f, 30f);
                                        //    if (ImGuiIV.ColorEdit4(string.Format("##{0}", info.Name), ref arr, eImGuiColorEditFlags.None))
                                        //        info.SetValue(script.TheScript, Color.FromArgb(Convert.ToByte(arr.W), Convert.ToByte(arr.X), Convert.ToByte(arr.Y), Convert.ToByte(arr.Z)));
                                        //    ImGuiIV.SameLine();
                                        //    ImGuiIV.HelpMarker(string.Format("This field is a {0}.", info.FieldType.Name));
                                        //}

                                    }

                                    ImGuiIV.EndChild();
                                }
                                else
                                {
                                    ImGuiIV.Text("This script has no public fields.");
                                }

                                ImGuiIV.EndTabItem();
                            }

                            ImGuiIV.EndTabBar();
                        }

                    }
                    else
                    {
                        ImGuiIV.Text("There are currently no active scripts.");
                    }

                    ImGuiIV.EndTabItem();
                }

                // About
                if (ImGuiIV.BeginTabItem("About"))
                {
 
                    // Some basic info
                    ImGuiIV.HelpMarker("Some information about IV-SDK .NET!");
                    ImGuiIV.SameLine();
                    ImGuiIV.SeparatorText("Info");
                    ImGuiIV.Text("IV-SDK .NET Version {0} by ItsClonkAndre", CLR.CLRBridge.Version);
#if DEBUG
                    ImGuiIV.Text("Build: Debug");
#else
                    ImGuiIV.Text("Build: Release");
#endif

                    ImGuiIV.Dummy(new Vector2(0f, 5f));
                    ImGuiIV.Text("Current IVSDKDotNetWrapper Version: {0}", main.CurrentWrapperVersion);

                    // Credits
                    ImGuiIV.Dummy(new Vector2(0f, 10f));
                    ImGuiIV.HelpMarker("Honorable mentions!");
                    ImGuiIV.SameLine();
                    ImGuiIV.SeparatorText("Credits");

                    Helper.AskToOpenWebPageButton("IV-SDK by Zolika1351", new Vector2(ImGuiIV.FloatMin, 0f), new Uri("https://github.com/Zolika1351/iv-sdk"));
                    Helper.AskToOpenWebPageButton("InGameTimecycEditor by akifle47", new Vector2(ImGuiIV.FloatMin, 0f), new Uri("https://github.com/akifle47/InGameTimecycEditor"));
                    Helper.AskToOpenWebPageButton("Dear ImGui by ocornut", new Vector2(ImGuiIV.FloatMin, 0f), new Uri("https://github.com/ocornut/imgui"));

                    // Links
                    ImGuiIV.Dummy(new Vector2(0f, 10f));
                    ImGuiIV.HelpMarker("Interesting links that you might want to check out!");
                    ImGuiIV.SameLine();
                    ImGuiIV.SeparatorText("Links");

                    ImGuiIV.Text("IV-SDK .NET on");
                    ImGuiIV.SameLine(0f, 5f);
                    Helper.AskToOpenWebPageButton("GitHub", Vector2.Zero, new Uri("https://github.com/ClonkAndre/IV-SDK-DotNet"));
                    ImGuiIV.SameLine(0f, 5f);
                    ImGuiIV.Text("or");
                    ImGuiIV.SameLine(0f, 5f);
                    Helper.AskToOpenWebPageButton("gtaforums", Vector2.Zero, new Uri("https://gtaforums.com/topic/986510-iv-sdk-net/"));
                    
                    ImGuiIV.Dummy(new Vector2(0f, 5f));
                    ImGuiIV.TextWrapped("Check out ItsClonkAndre's Workshop on gtaforums for IV-SDK .NET mods, or other GTA IV things!");
                    Helper.AskToOpenWebPageButton("ItsClonkAndre's Workshop", new Vector2(ImGuiIV.FloatMin, 0f), new Uri("https://gtaforums.com/topic/988909-itsclonkandres-workshop/"));

                    // Support
                    ImGuiIV.Dummy(new Vector2(0f, 10f));
                    ImGuiIV.HelpMarker("Check out who is a current member on Patreon or Ko-fi or become one yourself!");
                    ImGuiIV.SameLine();
                    ImGuiIV.SeparatorText("Support");

                    ImGuiIV.TextWrapped("Become a member today to have your name shown in one of the nodes below!");
                    Helper.AskToOpenWebPageButton("Patreon", new Vector2(ImGuiIV.FloatMin, 0f), new Uri("https://www.patreon.com/itsclonkandre"));
                    ImGuiIV.SameLine();
                    Helper.AskToOpenWebPageButton("Ko-fi", new Vector2(ImGuiIV.FloatMin, 0f), new Uri("https://ko-fi.com/itsclonkandre"));

                    ImGuiIV.Dummy(new Vector2(0f, 5f));

                    if (ImGuiIV.TreeNode("Tier 3"))
                    {
                        if (Main.Instance.TierThreeSupporters.Count == 0)
                        {
                            ImGuiIV.Text("-");
                        }
                        else
                        {
                            for (int i = 0; i < Main.Instance.TierThreeSupporters.Count; i++)
                            {
                                string name = Main.Instance.TierThreeSupporters[i];
                                ImGuiIV.Selectable(name);
                            }
                        }

                        ImGuiIV.TreePop();
                    }
                    if (ImGuiIV.TreeNode("Tier 2"))
                    {
                        if (Main.Instance.TierTwoSupporters.Count == 0)
                        {
                            ImGuiIV.Text("-");
                        }
                        else
                        {
                            for (int i = 0; i < Main.Instance.TierTwoSupporters.Count; i++)
                            {
                                string name = Main.Instance.TierTwoSupporters[i];
                                ImGuiIV.Selectable(name);
                            }
                        }

                        ImGuiIV.TreePop();
                    }
                    if (ImGuiIV.TreeNode("Tier 1"))
                    {
                        if (Main.Instance.TierOneSupporters.Count == 0)
                        {
                            ImGuiIV.Text("-");
                        }
                        else
                        {
                            for (int i = 0; i < Main.Instance.TierOneSupporters.Count; i++)
                            {
                                string name = Main.Instance.TierOneSupporters[i];
                                ImGuiIV.Selectable(name);
                            }
                        }

                        ImGuiIV.TreePop();
                    }

                    ImGuiIV.EndTabItem();
                }

                ImGuiIV.EndTabBar();
            }

            ImGuiIV.End();
        }

    }
}