<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProjectGuid>{9667CEE9-74D1-429C-9282-75B85495DC8B}</ProjectGuid>
    <OutputType>AppContainerExe</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>PeerConnectionClient</RootNamespace>
    <AssemblyName>PeerConnectionClient.Windows</AssemblyName>
    <DefaultLanguage>en-US</DefaultLanguage>
    <TargetPlatformVersion>8.1</TargetPlatformVersion>
    <MinimumVisualStudioVersion>12</MinimumVisualStudioVersion>
    <FileAlignment>512</FileAlignment>
    <SynthesizeLinkMetadata>true</SynthesizeLinkMetadata>
    <ProjectTypeGuids>{BC8A1FFA-BEE3-4634-8014-F334798102B3};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>
    <PackageCertificateKeyFile>PeerConnectionClient.Windows_TemporaryKey.pfx</PackageCertificateKeyFile>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    <PlatformTarget>AnyCPU</PlatformTarget>
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE;NETFX_CORE;WINDOWS_APP</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
    <PlatformTarget>AnyCPU</PlatformTarget>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE;NETFX_CORE;WINDOWS_APP</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Debug|ARM'">
    <DebugSymbols>true</DebugSymbols>
    <OutputPath>bin\ARM\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE;NETFX_CORE;WINDOWS_APP</DefineConstants>
    <NoWarn>;2008</NoWarn>
    <DebugType>full</DebugType>
    <PlatformTarget>ARM</PlatformTarget>
    <UseVSHostingProcess>false</UseVSHostingProcess>
    <ErrorReport>prompt</ErrorReport>
    <Prefer32Bit>true</Prefer32Bit>
  </PropertyGroup>
  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Release|ARM'">
    <OutputPath>bin\ARM\Release\</OutputPath>
    <DefineConstants>TRACE;NETFX_CORE;WINDOWS_APP</DefineConstants>
    <Optimize>true</Optimize>
    <NoWarn>;2008</NoWarn>
    <DebugType>pdbonly</DebugType>
    <PlatformTarget>ARM</PlatformTarget>
    <UseVSHostingProcess>false</UseVSHostingProcess>
    <ErrorReport>prompt</ErrorReport>
    <Prefer32Bit>true</Prefer32Bit>
  </PropertyGroup>
  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Debug|x64'">
    <DebugSymbols>true</DebugSymbols>
    <OutputPath>bin\x64\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE;NETFX_CORE;WINDOWS_APP</DefineConstants>
    <NoWarn>;2008</NoWarn>
    <DebugType>full</DebugType>
    <PlatformTarget>x64</PlatformTarget>
    <UseVSHostingProcess>false</UseVSHostingProcess>
    <ErrorReport>prompt</ErrorReport>
    <Prefer32Bit>true</Prefer32Bit>
  </PropertyGroup>
  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Release|x64'">
    <OutputPath>bin\x64\Release\</OutputPath>
    <DefineConstants>TRACE;NETFX_CORE;WINDOWS_APP</DefineConstants>
    <Optimize>true</Optimize>
    <NoWarn>;2008</NoWarn>
    <DebugType>pdbonly</DebugType>
    <PlatformTarget>x64</PlatformTarget>
    <UseVSHostingProcess>false</UseVSHostingProcess>
    <ErrorReport>prompt</ErrorReport>
    <Prefer32Bit>true</Prefer32Bit>
  </PropertyGroup>
  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Debug|x86'">
    <DebugSymbols>true</DebugSymbols>
    <OutputPath>bin\x86\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE;NETFX_CORE;WINDOWS_APP</DefineConstants>
    <NoWarn>;2008</NoWarn>
    <DebugType>full</DebugType>
    <PlatformTarget>x86</PlatformTarget>
    <UseVSHostingProcess>false</UseVSHostingProcess>
    <ErrorReport>prompt</ErrorReport>
    <Prefer32Bit>true</Prefer32Bit>
  </PropertyGroup>
  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Release|x86'">
    <OutputPath>bin\x86\Release\</OutputPath>
    <DefineConstants>TRACE;NETFX_CORE;WINDOWS_APP</DefineConstants>
    <Optimize>true</Optimize>
    <NoWarn>;2008</NoWarn>
    <DebugType>pdbonly</DebugType>
    <PlatformTarget>x86</PlatformTarget>
    <UseVSHostingProcess>false</UseVSHostingProcess>
    <ErrorReport>prompt</ErrorReport>
    <Prefer32Bit>true</Prefer32Bit>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="ExtendedSplashScreen.xaml.cs">
      <DependentUpon>ExtendedSplashScreen.xaml</DependentUpon>
    </Compile>
    <Compile Include="SettingsFlyouts\AboutSettingsFlyout.xaml.cs">
      <DependentUpon>AboutSettingsFlyout.xaml</DependentUpon>
    </Compile>
    <Compile Include="SettingsFlyouts\AudioVideoSettingsFlyout.xaml.cs">
      <DependentUpon>AudioVideoSettingsFlyout.xaml</DependentUpon>
    </Compile>
    <Compile Include="SettingsFlyouts\ConnectionSettingsFlyout.xaml.cs">
      <DependentUpon>ConnectionSettingsFlyout.xaml</DependentUpon>
    </Compile>
    <Compile Include="SettingsFlyouts\DebugSettingsFlyout.xaml.cs">
      <DependentUpon>DebugSettingsFlyout.xaml</DependentUpon>
    </Compile>
    <Compile Include="MainPage.xaml.cs">
      <DependentUpon>MainPage.xaml</DependentUpon>
    </Compile>
    <Compile Include="Properties\AssemblyInfo.cs" />
  </ItemGroup>
  <ItemGroup>
    <AppxManifest Include="Package.appxmanifest">
      <SubType>Designer</SubType>
    </AppxManifest>
    <None Include="packages.config" />
    <None Include="PeerConnectionClient.Windows_TemporaryKey.pfx" />
  </ItemGroup>
  <ItemGroup>
    <Content Include="Assets\Logo.scale-100.png" />
    <Content Include="Assets\SmallLogo.scale-100.png" />
    <Content Include="Assets\SplashScreen.scale-100.png" />
    <Content Include="Assets\StoreLogo.scale-100.png" />
  </ItemGroup>
  <ItemGroup>
    <Page Include="ExtendedSplashScreen.xaml">
      <SubType>Designer</SubType>
      <Generator>MSBuild:Compile</Generator>
    </Page>
    <Page Include="SettingsFlyouts\AboutSettingsFlyout.xaml">
      <SubType>Designer</SubType>
      <Generator>MSBuild:Compile</Generator>
    </Page>
    <Page Include="SettingsFlyouts\AudioVideoSettingsFlyout.xaml">
      <SubType>Designer</SubType>
      <Generator>MSBuild:Compile</Generator>
    </Page>
    <Page Include="SettingsFlyouts\ConnectionSettingsFlyout.xaml">
      <SubType>Designer</SubType>
      <Generator>MSBuild:Compile</Generator>
    </Page>
    <Page Include="SettingsFlyouts\DebugSettingsFlyout.xaml">
      <SubType>Designer</SubType>
      <Generator>MSBuild:Compile</Generator>
    </Page>
    <Page Include="MainPage.xaml">
      <Generator>MSBuild:Compile</Generator>
      <SubType>Designer</SubType>
    </Page>
  </ItemGroup>
  <ItemGroup>
    <SDKReference Include="Microsoft.VCLibs, Version=12.0">
      <Name>Microsoft Visual C++ 2013 Runtime Package for Windows</Name>
    </SDKReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\..\src\webrtc\build\WinRT_gyp\Api\webrtc_winrt_api.vcxproj">
      <Project>{08a72a31-ed29-591b-a715-7842ad71fd3d}</Project>
      <Name>webrtc_winrt_api</Name>
    </ProjectReference>
  </ItemGroup>
  <ItemGroup>
    <Reference Include="HockeyApp">
      <HintPath>..\packages\HockeySDK.WINRT.2.2.2\lib\portable-win81\HockeyApp.dll</HintPath>
    </Reference>
    <Reference Include="HockeyAppPCL">
      <HintPath>..\packages\HockeySDK.Core.2.2.2\lib\portable-net45+win8+wp8+wpa81+win81\HockeyAppPCL.dll</HintPath>
    </Reference>
  </ItemGroup>
  <PropertyGroup Condition=" '$(VisualStudioVersion)' == '' or '$(VisualStudioVersion)' &lt; '12.0' ">
    <VisualStudioVersion>12.0</VisualStudioVersion>
  </PropertyGroup>
  <Import Project="..\PeerConnectionClient.Shared\PeerConnectionClient.Shared.projitems" Label="Shared" />
  <Import Project="$(MSBuildExtensionsPath)\Microsoft\WindowsXaml\v$(VisualStudioVersion)\Microsoft.Windows.UI.Xaml.CSharp.targets" />
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name="BeforeBuild">
  </Target>
  <Target Name="AfterBuild">
  </Target>
  -->
</Project>
