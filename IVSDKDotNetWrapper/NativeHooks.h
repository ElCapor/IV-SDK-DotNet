#pragma once

namespace IVSDKDotNet
{
	namespace Native
	{

		/// <summary>
		/// Some native function hooks which you can use to do stuff before a target native is called.
		/// </summary>
		public ref class Hooks
		{
		public: // Delegates
			delegate bool DefaultHookDelegate();

			delegate bool SingleStringDelegate(String^ str);
			delegate bool SingleIntDelegate(int i);
			delegate bool SingleFloatDelegate(float f);
			delegate bool SingleBoolDelegate(bool b);

		public: // Events
			/// <summary>The PRINTSTRING native is normally not functional and is supposed to log some debug stuff for e.g. missions.</summary>
			static event SingleStringDelegate^ PrintStringHook;
			/// <summary>The PRINTINT native is normally not functional and is supposed to log some debug stuff for e.g. missions.</summary>
			static event SingleIntDelegate^ PrintIntHook;
			/// <summary>The PRINTFLOAT native is normally not functional and is supposed to log some debug stuff for e.g. missions.</summary>
			static event SingleFloatDelegate^ PrintFloatHook;

			static event DefaultHookDelegate^ ShowSignInUIHook;
			static event SingleStringDelegate^ NetworkShowFriendProfileUIHook;

			static event SingleIntDelegate^ AwardAchievementHook;
			static event SingleBoolDelegate^ EnableChaseAudioHook;

		internal: // Event Raisers
			static bool RaisePrintStringHook(String^ str)
			{
				return PrintStringHook(str);
			}
			static bool RaisePrintIntHook(int num)
			{
				return PrintIntHook(num);
			}
			static bool RaisePrintFloatHook(float num)
			{
				return PrintFloatHook(num);
			}

			static bool RaiseShowSignInUIHook()
			{
				return ShowSignInUIHook();
			}
			static bool RaiseNetworkShowFriendProfileUIHook(String^ friendName)
			{
				return NetworkShowFriendProfileUIHook(friendName);
			}

			static bool RaiseAwardAchievementHook(int id)
			{
				return AwardAchievementHook(id);
			}
			static bool RaiseEnableChaseAudioHook(bool enable)
			{
				return EnableChaseAudioHook(enable);
			}

		internal:
			static property bool Initialized
			{
			internal:
				bool get() { return wasInitialized; }
				void set(bool value) { wasInitialized = value; }
			}

		internal:
			static void Initialize();
			static void Cleanup();

		internal:
			ref class InstalledHook
			{
			public:
				InstalledHook(eNativeHash nativeHash, DWORD hookFunc)
				{
					NativeHash = nativeHash;
					HookFunction = hookFunc;
					Hook();
				}

			public:
				void Unhook()
				{
					if (!TrampolinePtr)
						return;

					CPatcher::UninstallDetourPatch(TrampolinePtr, CTheScripts::FindNativeAddress((uint32_t)NativeHash));
					TrampolinePtr = nullptr;
				}
				void Hook()
				{
					if (TrampolinePtr)
						return;

					TrampolinePtr = CPatcher::InstallJmpPatch(CTheScripts::FindNativeAddress((uint32_t)NativeHash), HookFunction, 5);
				}

			public:
				eNativeHash NativeHash;
				void* TrampolinePtr;
				DWORD HookFunction;
			};

		internal:
			static InstalledHook^ FindInstalledHook(eNativeHash byNativeHash);
			static void AddHook(InstalledHook^ hook);
			static bool VerifyHooks();

		private:
			static List<InstalledHook^>^ installedHooks;
			static bool wasInitialized;
		};

		// Internal class which contains all the detour hook methods
		class NativeHookFunctions
		{
		public:
			static bool Initialize();

		private:
			static void DoHooking();

		private:
			static void NATIVE_PRINTSTRING(IVNativeCallContext* pNativeContext);
			static void NATIVE_PRINTINT(IVNativeCallContext* pNativeContext);
			static void NATIVE_PRINTFLOAT(IVNativeCallContext* pNativeContext);

			static void NATIVE_SHOW_SIGNIN_UI(IVNativeCallContext* pNativeContext);
			static void NATIVE_NETWORK_SHOW_FRIEND_PROFILE_UI(IVNativeCallContext* pNativeContext);

			static void NATIVE_AWARD_ACHIEVEMENT(IVNativeCallContext* pNativeContext);
			static void NATIVE_ENABLE_CHASE_AUDIO(IVNativeCallContext* pNativeContext);
		};

	}
}