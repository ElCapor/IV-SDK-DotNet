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
