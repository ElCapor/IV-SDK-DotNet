#include <cstdint>
#include <pattern/pattern.hpp>
#include <eyestep/eyestep.h>
#include <eyestep/eyestep_utility.h>

#define PATTERN PatternHolder

// pattern holder
struct PatternHolder
{
public:
    const char* aob;//ida format aob
    void* target; // target address

    PatternHolder(const char* aob = "", void* target = nullptr) : aob(aob), target(target)
    {
        // init the structure
    }

    void* find(ptrdiff_t offset)
    {
        target = hook::pattern(aob).get_first(offset);
        return target;
    }

    void* get(std::uint32_t index, ptrdiff_t offset)
    {
        target = hook::pattern(aob).get(index).get<void>(offset);
        return target;
    }
};


namespace patterns
{
    namespace patterns
    {
        namespace events
        {
            PATTERN automobile = "8B CE E8 ? ? ? ? 8B CE E8 ? ? ? ? 8B 46 28 C1 E8 0A";
            PATTERN pad = "8B CE E8 ? ? ? ? 81 C6 84 3A 00 00";
            PATTERN camera = "8B CE E8 ? ? ? ? 5F 5E B0 01 5B C3";
            PATTERN drawing = "E8 ? ? ? ? 83 C4 08 E8 ? ? ? ? E8 ? ? ? ? 83 3D ? ? ? ? 00 74 ?";
            PATTERN inGameStartup = "56 E8 ? ? ? ? A3 ? ? ? ? 89 15 ? ? ? ? E8 ? ? ? ?";
            PATTERN gameEvent = "83 ? ? 3B ? 7D ? 8B 1F 8B B6 80 00 00 00 8B ? 69 ? 88 00 00 00"; // this requires a check for D:E
            PATTERN FuncAboveProcessHook = "55 8B EC 83 E4 F0 81 EC 84 00 00 00 83 3D ? ? ? ? 00"; // aob to the func on top of process hook
            namespace hard { // these are hard to get
                std::uint32_t GetPopulationConfigCall() // aka load event prior to the game launching
                {
                    auto xref_result = EyeStep::scanner::scan_xrefs("common:/data/pedVariations.dat");
                    auto func = xref_result[0];
                    // we can't get prologue since it's a near function
                    func -= 7;
                    auto new_xref = EyeStep::scanner::scan_xrefs(func);
                    return new_xref[0];
                }

                std::uint32_t GetMountDeviceCall()
                {
                    auto xref_result = EyeStep::scanner::scan_xrefs("commonimg:/");//
                    auto func = xref_result[0];
                    bool done = false;
                    while (!done) // a bit hacky but i got no choice ong
                    {
                        func--;
                        if (func % 16 == 0)
                        {
                            if (EyeStep::util::readByte(func) == 0x81 && EyeStep::util::readByte(func + 1) == 0xEC)
                                done = true;
                        }
                    }
                    xref_result = EyeStep::scanner::scan_xrefs(func);
                    func = xref_result[0];
                    return func;
                }

                std::uint32_t GetLoadEventCall()
                {
                    auto pattern = hook::pattern(gameEvent.aob);
                    std::uint32_t address = 0;
                    if (pattern.size() > 1)
                    {
                        address = (std::uint32_t)pattern.get(2).get<std::uint32_t>(0);
                    }
                    else {
                        address = (std::uint32_t)pattern.get_first(0);
                    }

                    bool done = false;
                    while (!done) // a bit hacky but i got no choice ong
                    {
                        address--;
                        if (address % 16 == 0)
                        {
                            if ((EyeStep::util::readByte(address) == 0x81 || EyeStep::util::readByte(address) == 0x83) && EyeStep::util::readByte(address + 1) == 0xEC)
                                done = true;
                        }
                    }
                    auto xref_result = EyeStep::scanner::scan_xrefs(address);
                    address = xref_result[0];
                    return address;
                }

                std::uint32_t GetProcessHookAddres()
                {
                    auto abovefunc = FuncAboveProcessHook.find(0);
                    auto xref_results = EyeStep::scanner::scan_xrefs((std::uint32_t)abovefunc);
                    auto callAbove = xref_results[0];
                    return callAbove + 5;
                }
            }
        }

        namespace pools {
            PATTERN ms_PedPool = "64 A1 2C 00 00 00 6A 00 8B 00 6A 10 8B 48 08 6A 1C 8B 01 FF 50 08"; // index 2
            std::uint32_t GetPedPool()
            {
                std::uint32_t ret = 0;
                auto ptr = (std::uint32_t)hook::pattern(ms_PedPool.aob).get(2).get<void>(36);
                ret = EyeStep::util::readInt(ptr + 1);
                return ret;
            }
        }
    }
}