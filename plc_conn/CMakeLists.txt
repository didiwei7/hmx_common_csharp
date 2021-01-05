project(plc_conn LANGUAGES CSharp)

file (GLOB src "./*.cs")

add_executable(${PROJECT_NAME} ${src})

set_property(TARGET ${PROJECT_NAME} PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.7.2") 
set_property(TARGET ${PROJECT_NAME} PROPERTY WIN32_EXECUTABLE TRUE)
set_property(TARGET ${PROJECT_NAME} PROPERTY VS_DOTNET_REFERENCES
    "Microsoft.CSharp"
    "System"
    "System.Core"
    "System.Collections.Generic"
    "System.Text"
    "System.Linq"
    "System.Net"
    "PresentationCore"
    "PresentationFramework"
    "System.Data"
    "System.Data.DataSetExtensions"
    "System.Net.Http"
    "System.Xaml"
    "System.Xml"
    "System.Xml.Linq"
    "WindowsBase")