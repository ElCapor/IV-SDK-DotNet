namespace AddressSetter
{
	static uint32_t gBaseAddress;
	static bool bAddressesRead = false;

	static inline uint32_t GetVersionFromEXE()
	{
		TCHAR szFileName[MAX_PATH];

		GetModuleFileName(NULL, szFileName, MAX_PATH);

		DWORD  verHandle = 0;
		UINT   size = 0;
		LPBYTE lpBuffer = NULL;
		DWORD  verSize = GetFileVersionInfoSize(szFileName, &verHandle);

		if (verSize != NULL)
		{
			LPSTR verData = new char[verSize];

			if (GetFileVersionInfo(szFileName, verHandle, verSize, verData))
			{
				if (VerQueryValue(verData, TEXT("\\"), (VOID FAR * FAR*) & lpBuffer, &size))
				{
					if (size)
					{
						VS_FIXEDFILEINFO* verInfo = (VS_FIXEDFILEINFO*)lpBuffer;
						if (verInfo->dwSignature == 0xfeef04bd)
						{
							std::string str = std::to_string((verInfo->dwFileVersionMS >> 16) & 0xffff);
							str += std::to_string((verInfo->dwFileVersionMS >> 0) & 0xffff);
							str += std::to_string((verInfo->dwFileVersionLS >> 16) & 0xffff);
							str += std::to_string((verInfo->dwFileVersionLS >> 0) & 0xffff);
							delete[] verData;
							return std::stoi(str);
						}
					}
				}
			}
			delete[] verData;
		}
		return 0;
	}

	static inline void DetermineVersion()
	{
		switch (GetVersionFromEXE())
		{
		case 1070:
			plugin::gameVer = plugin::VERSION_1070;
			break;
		case 1080:
			plugin::gameVer = plugin::VERSION_1080;
			break;
		case 12043:
			plugin::gameVer = plugin::VERSION_CE;
			break;
		case 12053:
			plugin::gameVer = plugin::VERSION_CE;
			break;
		default:
			plugin::gameVer = plugin::VERSION_NONE;
			break;
		}
	}

	static inline void Init()
	{
		DetermineVersion();
		gBaseAddress = (uint32_t)GetModuleHandle(NULL);
		bAddressesRead = true;
	}

	// Note that the base address is added here and 0x400000 is not subtracted, so rebase your .idb to 0x0 or subtract it yourself
	template<typename T>
	// ElCapor - I temporarily added an optional addrCE argument to test CE without having to make this change breaking
	static inline T& GetRef(uint32_t addr1070, uint32_t addr1080, uint32_t addrCE = 0, bool useBaseAddress = true)
	{
		if (!bAddressesRead)
			Init();

		switch(plugin::gameVer)
		{
		case plugin::VERSION_1080: return *reinterpret_cast<T*>(gBaseAddress + addr1080); break;
		case plugin::VERSION_1070: return *reinterpret_cast<T*>(gBaseAddress + addr1070); break;
		case plugin::VERSION_CE: return *reinterpret_cast<T*>(useBaseAddress ? gBaseAddress : 0 + addrCE); break;
		}

		return *reinterpret_cast<T*>(nullptr);
	}

	// addreCe added here too
	static inline uint32_t Get(uint32_t addr1070, uint32_t addr1080, uint32_t addrCE = 0, bool useBaseAddress = true)
	{
		if (!bAddressesRead)
			Init();

		switch(plugin::gameVer)
		{
		case plugin::VERSION_1080: return gBaseAddress + addr1080; break;
		case plugin::VERSION_1070: return gBaseAddress + addr1070; break;
		case plugin::VERSION_CE: return useBaseAddress ? gBaseAddress : 0 + addrCE; break;
		}

		return 0;
	}
}