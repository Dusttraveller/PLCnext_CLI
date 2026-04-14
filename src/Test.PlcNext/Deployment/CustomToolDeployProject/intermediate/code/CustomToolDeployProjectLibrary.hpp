#pragma once
#include "Arp/System/Core/Arp.h"
#if ARP_ABI_VERSION_MAJOR < 2
#include "Arp/System/Core/AppDomain.hpp"
#include "Arp/System/Core/Singleton.hxx"
#include "Arp/System/Core/Library.h"
#endif
#include "Arp/Plc/Commons/Meta/MetaLibraryBase.hpp"
#include "Arp/Plc/Commons/Meta/TypeSystem/TypeDomain.hpp"

namespace CustomToolDeployProject
{

#if ARP_ABI_VERSION_MAJOR < 2
using namespace Arp::System::Acf;
#else
using namespace Arp::Base::Acf::Commons;
#endif
using namespace Arp::Plc::Commons::Meta;
using namespace Arp::Plc::Commons::Meta::TypeSystem;

class CustomToolDeployProjectLibrary : public MetaLibraryBase
#if ARP_ABI_VERSION_MAJOR < 2
	, public Singleton<CustomToolDeployProjectLibrary>
#endif
{
#if ARP_ABI_VERSION_MAJOR < 2
public: // typedefs
    typedef Singleton<CustomToolDeployProjectLibrary> SingletonBase;

public: // construction/destruction
    CustomToolDeployProjectLibrary(AppDomain& appDomain);
    virtual ~CustomToolDeployProjectLibrary() = default;

public: // static operations (called through reflection)
    static void Main(AppDomain& appDomain);
#else
public: // construction/destruction 
    CustomToolDeployProjectLibrary(void);

public: // static singleton operations
    static CustomToolDeployProjectLibrary& GetInstance();
#endif

private: // methods
    void InitializeTypeDomain();

#if ARP_ABI_VERSION_MAJOR < 2
private: // deleted methods
    CustomToolDeployProjectLibrary(const CustomToolDeployProjectLibrary& arg) = delete;
    CustomToolDeployProjectLibrary& operator= (const CustomToolDeployProjectLibrary& arg) = delete;
#endif

private:  // fields
    TypeDomain typeDomain;
};

#if ARP_ABI_VERSION_MAJOR < 2
extern "C" ARP_CXX_SYMBOL_EXPORT ILibrary& ArpDynamicLibraryMain(AppDomain& appDomain);
#endif
} // end of namespace CustomToolDeployProject
