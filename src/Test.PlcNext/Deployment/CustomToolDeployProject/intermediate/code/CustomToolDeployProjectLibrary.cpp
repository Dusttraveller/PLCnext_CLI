#include "CustomToolDeployProjectLibrary.hpp"
#if ARP_ABI_VERSION_MAJOR < 2
#include "Arp/System/Core/CommonTypeName.hxx"
#else
#include "Arp/Base/Core/CommonTypeName.hxx"
#include "CustomToolDeployProjectLibraryInfo.hpp"
#endif
#include "Arp/Plc/Commons/Meta/TypeSystem/TypeSystem.h"
#include "CustomToolDeployProjectComponent.hpp"

namespace CustomToolDeployProject
{

#if ARP_ABI_VERSION_MAJOR < 2
CustomToolDeployProjectLibrary::CustomToolDeployProjectLibrary(AppDomain& appDomain)
    : MetaLibraryBase(appDomain, ARP_VERSION_CURRENT, typeDomain)
    , typeDomain(CommonTypeName<CustomToolDeployProjectLibrary>().GetNamespace())
#else
CustomToolDeployProjectLibrary::CustomToolDeployProjectLibrary()
    : MetaLibraryBase(CustomToolDeployProjectLibraryVersion, typeDomain)
    , typeDomain(CommonTypeName<CustomToolDeployProjectLibrary>().GetNamespace())
#endif
{
#if ARP_ABI_VERSION_MAJOR < 2
    this->componentFactory.AddFactoryMethod(CommonTypeName<::CustomToolDeployProject::CustomToolDeployProjectComponent>(), &::CustomToolDeployProject::CustomToolDeployProjectComponent::Create);
#else
#if ARP_ABI_VERSION_MAJOR >=3 || (ARP_ABI_VERSION_MAJOR >= 2 && ARP_ABI_VERSION_MINOR >= 1)   // from firmware 2026.0
   this->SetSdkBuildInfo(ARP_VERSION_BUILT, ARP_ABI_VERSION_NAME, ARP_TARGET_IDENTIFIER);
#endif 
 
    this->AddComponentType<::CustomToolDeployProject::CustomToolDeployProjectComponent>();
#endif
    this->InitializeTypeDomain();
}

#if ARP_ABI_VERSION_MAJOR < 2
void CustomToolDeployProjectLibrary::Main(AppDomain& appDomain)
{
    SingletonBase::CreateInstance(appDomain);
}
#else
CustomToolDeployProjectLibrary& CustomToolDeployProjectLibrary::GetInstance()
{
    static CustomToolDeployProjectLibrary instance;
    return instance;
}
#endif


#if ARP_ABI_VERSION_MAJOR < 2
extern "C" ARP_CXX_SYMBOL_EXPORT ILibrary& ArpDynamicLibraryMain(AppDomain& appDomain)
{
    CustomToolDeployProjectLibrary::Main(appDomain);
    return  CustomToolDeployProjectLibrary::GetInstance();
}
} // end of namespace CustomToolDeployProject
#else
} // end of namespace CustomToolDeployProject
extern "C" ARP_EXPORT Arp::Base::Acf::Commons::ILibrary& CustomToolDeployProject_MainEntry()
{
    return  CustomToolDeployProject::CustomToolDeployProjectLibrary::GetInstance();
}
#endif

