#include "pch.h"
#include "CLRBridge.h"

using namespace System::Diagnostics;
using namespace IVSDKDotNet;
using namespace IVSDKDotNet::Manager;

WNDPROC originalWndProc = nullptr;

LRESULT __stdcall WndProcHook(const HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	// Window starts being destroyed
	if (uMsg == WM_DESTROY)
	{
		// Cleanup internal things
		CLR::CLRBridge::Cleanup();

		// Invoke event
		RAGE::RaiseOnExit();
	}

	// Invoke event
	RAGE::RaiseOnWndProcMessageReceived(IntPtr(hWnd), uMsg, UIntPtr(wParam), IntPtr(lParam));

	// Invoke ImGui WndProc
	if (ImGuiDraw::OnWndProc(hWnd, uMsg, wParam, lParam))
		return true;

	// Call the original function
	return CallWindowProc(originalWndProc, hWnd, uMsg, wParam, lParam);
}

namespace CLR
{

	void CLRBridge::Initialize(uint32_t version, uint32_t baseAddress)
	{
		if (CLRBridge::IsBridgeDisabled)
			return;

		// Set paths
		IVSDKDotNetPath =			Application::StartupPath + "\\IVSDKDotNet";
		IVSDKDotNetBinaryPath =		IVSDKDotNetPath + "\\bin";
		IVSDKDotNetDataPath =		IVSDKDotNetPath + "\\data";
		IVSDKDotNetLogsPath =		IVSDKDotNetPath + "\\logs";
		IVSDKDotNetScriptsPath =	IVSDKDotNetPath + "\\scripts";
		String^ managerScriptPath = IVSDKDotNetPath + "\\IVSDKDotNet.Manager.dll";

		// Set current log file name
		DateTime dtNow = DateTime::Now;
		CurrentLogFileName = String::Format("IVSDKDotNet@{0}{1}{2}@{3}.log", dtNow.Day, dtNow.Month, dtNow.Year, dtNow.Millisecond);

		// Initialize and try loading the IVSDKDotNet settings file
		Settings = gcnew SettingsFile(IVSDKDotNetPath + "\\config.ini");
		bool settingsLoadResult = Settings->Load();
		
		// Initialize logger
		Logger::Initialize();

		// Force english culture of current thread
		System::Globalization::CultureInfo^ cultureInfo = gcnew System::Globalization::CultureInfo("en-US");
		System::Threading::Thread::CurrentThread->CurrentCulture = cultureInfo;
		System::Threading::Thread::CurrentThread->CurrentUICulture = cultureInfo;

		// Print about text to console
#if _DEBUG
		Logger::Log(String::Format("IV-SDK .NET DEBUG version {0} by ItsClonkAndre", Version));

		// Launch debugger
		if (Settings->GetBoolean("DEBUG", "LaunchDebugger", false))
			Debugger::Launch();
#else
		Logger::Log(String::Format("IV-SDK .NET Release version {0} by ItsClonkAndre", Version));
#endif

		// Log settings file load result
		if (settingsLoadResult)
			Logger::Log("Loaded IV-SDK .NET config.ini!");
		else
			Logger::LogWarning("Could not load IV-SDK .NET config.ini! Using default settings.");

		// Do checks
		if (!File::Exists(managerScriptPath))
		{
			Logger::LogError("Could not initialize IV-SDK .NET because the IVSDKDotNet.Manager.dll file was not found inside the IVSDKDotNet directory!", false);
			goto ERR;
		}
		if (!File::Exists(IVSDKDotNetDataPath + "\\public-sans.regular.ttf"))
		{
			Logger::LogError("Could not initialize IV-SDK .NET because the public-sans.regular.ttf file was not found inside the IVSDKDotNet->Data directory!", false);
			goto ERR;
		}

		if (File::Exists("ScriptHookDotNet.dll"))
		{
			switch (MessageBox::Show("We've detected that you have a ScriptHookDotNet.dll file within your main directory of GTA IV. " + 
				"Since version 1.2 of IV-SDK .NET, a modified version of ScriptHookDotNet.dll is already included with IV-SDK .NET, and the one you have in your main directory might conflict with the one already included. " +
				"Would you like to close the game now and remove the old ScriptHookDotNet?", "IV-SDK .NET", MessageBoxButtons::YesNo, MessageBoxIcon::Warning))
			{
				case DialogResult::Yes:
					Environment::Exit(0);
					break;
			}
		}
		else
		{
			if (File::Exists("ScriptHookDotNet.asi"))
			{
				switch (MessageBox::Show("We've detected that you have a ScriptHookDotNet.asi file within your main directory of GTA IV. " +
					"Since version 1.2 of IV-SDK .NET, it can now load ScriptHookDotNet mods too and it makes the old ScriptHookDotNet obsolete! " +
					"Would you like to close the game now and remove the old ScriptHookDotNet?", "IV-SDK .NET", MessageBoxButtons::YesNo, MessageBoxIcon::Warning))
				{
					case DialogResult::Yes:
						Environment::Exit(0);
						break;
				}
			}
		}

		// Initialize Memory Access stuff
		MemoryAccess::Initialise(version, baseAddress);

		// Get the GTA IV Process
		TheGTAProcess = Process::GetCurrentProcess();

		// Wait till process has a MainWindowHandle and for g_pDirect3DDevice to return a valid pointer
		while (TheGTAProcess->MainWindowHandle == IntPtr::Zero)
			Sleep(100);
		while (rage::g_pDirect3DDevice == nullptr)
			Sleep(100);

		// Get the ImGui style
		String^ style = Settings->GetValue("Style", "ImGuiStyle", "dark")->ToLower();

		if (style == "dark")
			ImGuiIV::SelectedStyle = 0;
		else if (style == "classic")
			ImGuiIV::SelectedStyle = 1;
		else if (style == "light")
			ImGuiIV::SelectedStyle = 2;
		else
			ImGuiIV::SelectedStyle = 0;

		// Initialize stuff
		MH_STATUS minHookStatus = MH_Initialize();

		if (minHookStatus != MH_STATUS::MH_OK)
			Logger::LogError(String::Format("[MinHook] Failed to initialize! Details: {0}", gcnew String(MH_StatusToString(minHookStatus))));

		// Hook stuff
		if (DXHook::Initialize())
			Logger::LogDebug("[DXHook] Done!");
		if (DirectInputHook::Initialize())
			Logger::LogDebug("[DirectInputHook] Done!");

		originalWndProc = (WNDPROC)SetWindowLongPtr(FindWindow(L"grcWindow", L"GTAIV"), GWLP_WNDPROC, (LONG_PTR)WndProcHook);

		// Enable hooks
		minHookStatus = MH_EnableHook(MH_ALL_HOOKS);

		if (minHookStatus != MH_STATUS::MH_OK)
			Logger::LogError(String::Format("[MinHook] Failed to enable Hooks! Details: {0}", gcnew String(MH_StatusToString(minHookStatus))));

		// 300 ms was too little out of the sudden :thinking:
		Sleep(1100);

		// Load manager script
		try
		{
			Assembly^ assembly = Assembly::UnsafeLoadFrom(managerScriptPath);

			// Get types from assembly
			array<Type^>^ containedTypes = assembly->GetTypes();
			for (int i = 0; i < containedTypes->Length; i++)
			{
				Type^ containedType = containedTypes[i];

				if (containedType->IsSubclassOf(ManagerScript::typeid))
				{
					// Create new instance of type for assembly
					ManagerScript^ ms = (ManagerScript^)assembly->CreateInstance(containedType->FullName);

					if (ms)
					{
						// Set static instance
						ManagerScript::s_Instance = ms;

						// Apply Settings
						ms->ApplySettings(Settings);

						// Create and set dummy script for manager
						ms->SetDummyScript(gcnew Script(Guid("00000000-0000-0000-0000-000000000001")));

						// Load scripts
						if (!Settings->GetBoolean("DEBUG", "DisableScriptLoadOnStartup", false))
							ms->LoadScripts();

						return;
					}

					// This line of code should never be reached
					Logger::LogError("'ms' is nullptr in CLR::CLRBridge::Initialize!");
				}
			}

		}
		catch (ReflectionTypeLoadException^ ex)
		{
			array<Exception^>^ exs = ex->LoaderExceptions;
			for (int i = 0; i < exs->Length; i++)
			{
				Exception^ e = exs[i];
				Logger::LogError(String::Format("A ReflectionTypeLoadException occured while trying to load the IV-SDK .NET Manager. Details: {0}", e->ToString()), false);
			}

#if _DEBUG
			if (Debugger::IsAttached)
				throw;
#endif // _DEBUG
		}
		catch (Exception^ ex)
		{
			Logger::LogError(String::Format("An exception occured while trying to load the IV-SDK .NET Manager. Details: {0}", ex->ToString()));
		
#if _DEBUG
			if (Debugger::IsAttached)
				throw;
#endif // _DEBUG
		}

ERR:

		CLRBridge::IsBridgeDisabled = true;
		SHOW_WARN_MESSAGE("IV-SDK .NET got disabled due to errors while trying to load it. Check the 'IVSDKDotNet.log' file, to find out what went wrong.");
	}

	void CLRBridge::InvokeTickEvents()
	{
		if (CLRBridge::IsBridgeDisabled)
			return;

		if (ManagerScript::s_Instance)
			ManagerScript::s_Instance->RaiseTick();
	}
	void CLRBridge::InvokeGameLoadEvents()
	{
		if (CLRBridge::IsBridgeDisabled)
			return;

		if (ManagerScript::s_Instance)
			ManagerScript::s_Instance->RaiseGameLoad();
	}
	void CLRBridge::InvokeGameLoadPriorityEvents()
	{
		if (CLRBridge::IsBridgeDisabled)
			return;

		if (ManagerScript::s_Instance)
			ManagerScript::s_Instance->RaiseGameLoadPriority();

		// Initialize native function hooks
		Native::Hooks::Initialize();
		if (Native::NativeHookFunctions::Initialize())
			Logger::LogDebug("[NativeHook] Done!");
	}
	void CLRBridge::InvokeMountDeviceEvents()
	{
		if (CLRBridge::IsBridgeDisabled)
			return;
		
		if (ManagerScript::s_Instance)
			ManagerScript::s_Instance->RaiseMountDevice();
	}
	void CLRBridge::InvokeDrawingEvents()
	{
		if (CLRBridge::IsBridgeDisabled)
			return;

		if (ManagerScript::s_Instance)
			ManagerScript::s_Instance->RaiseDrawing();
	}
	void CLRBridge::InvokeProcessCameraEvents()
	{
		if (CLRBridge::IsBridgeDisabled)
			return;

		if (ManagerScript::s_Instance)
			ManagerScript::s_Instance->RaiseProcessCamera();
	}
	void CLRBridge::InvokeProcessAutomobileEvents(uint32_t* vehPtr)
	{
		if (CLRBridge::IsBridgeDisabled)
			return;

		if (ManagerScript::s_Instance)
			ManagerScript::s_Instance->RaiseProcessAutomobile(UIntPtr(vehPtr));
	}
	void CLRBridge::InvokeProcessPadEvents(uint32_t* padPtr)
	{
		if (CLRBridge::IsBridgeDisabled)
			return;

		if (ManagerScript::s_Instance)
		{
			// ManagerScript things
			if (ManagerScript::s_Instance->SwitchImGuiForceCursorProperty)
			{
				ImGuiIV::ForceCursor = !ImGuiIV::ForceCursor;
				ManagerScript::s_Instance->SwitchImGuiForceCursorProperty = false;
			}

			// Raise event for all subscribers
			ManagerScript::s_Instance->RaiseProcessPad(UIntPtr(padPtr));
		}
	}
	void CLRBridge::InvokeIngameStartupEvent()
	{
		if (CLRBridge::IsBridgeDisabled)
			return;

		if (ManagerScript::s_Instance)
			ManagerScript::s_Instance->RaiseIngameStartup();
	}

	void CLRBridge::Cleanup()
	{
		Logger::Log("Starting to clean up...");

		// Start cleanup in manager script
		if (ManagerScript::s_Instance)
			ManagerScript::s_Instance->Cleanup();

		// MinHook
		MH_STATUS minHookStatus = MH_DisableHook(MH_ALL_HOOKS);

		if (minHookStatus != MH_STATUS::MH_OK)
			Logger::LogError(String::Format("[MinHook] Failed to disable Hooks! Details: {0}", gcnew String(MH_StatusToString(minHookStatus))), false);

		minHookStatus = MH_Uninitialize();

		if (minHookStatus != MH_STATUS::MH_OK)
			Logger::LogError(String::Format("[MinHook] Failed to uninitialize! Details: {0}", gcnew String(MH_StatusToString(minHookStatus))), false);

		// ImGui
		ImGuiDraw::UninitializeImGui();

		Logger::LogDebug("Cleanup finished!");

		CanTerminate = true;
	}

}