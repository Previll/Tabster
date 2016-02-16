!include "FileAssociation.nsh"

!define PRODUCT_NAME "Tabster"
!define PRODUCT_VERSION "${APPLICATION_VERSION}"
!define PRODUCT_PUBLISHER "Nate Shoffner"
!define PRODUCT_WEB_SITE "http://nateshoffner.com"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\Tabster.exe"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

SetCompressor lzma

; MUI 1.67 compatible ------
!include "MUI.nsh"

; MUI Settings

!define MUI_ABORTWARNING
!define MUI_ICON "Icon.ico"
!define MUI_UNICON "Icon.ico"
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "License.txt"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\Tabster.exe"
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"

; MUI end ------

Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "Tabster ${PRODUCT_VERSION} Setup.exe"
InstallDir "$PROGRAMFILES\Tabster"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails show
ShowUnInstDetails show

Section "MainSection" SEC01
  SetOutPath "$INSTDIR"
  SetOverwrite ifnewer

  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Tabster.exe"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Tabster.Core.dll"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Tabster.Data.dll"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Tabster.Printing.dll"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Tabster.WinForms.dll"

  ; third-party references  
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\ObjectListView.dll"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\System.Data.SQLite.dll"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\x86\SQLite.Interop.dll"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\log4net.dll"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Newtonsoft.Json.dll"
  
  CreateShortCut "$DESKTOP\Tabster.lnk" "$INSTDIR\Tabster.exe"
  CreateDirectory "$SMPROGRAMS\Tabster"
  CreateShortCut "$SMPROGRAMS\Tabster\Tabster.lnk" "$INSTDIR\Tabster.exe"
  CreateShortCut "$SMPROGRAMS\Tabster\Tabster (Safe Mode).lnk" "$INSTDIR\Tabster.exe" "-safe-mode"
  
  !define PLUGIN_DIRECTORY "$INSTDIR\Plugins"
  !define PLUGIN_FILETYPES_DIRECTORY "${PLUGIN_DIRECTORY}\FileTypes"
  !define PLUGIN_SEARCHING_DIRECTORY "${PLUGIN_DIRECTORY}\Searching"
  
  !define RESOURCES_DIRECTORY "$INSTDIR\Resources"
  !define FONTS_DIRECTORY "${RESOURCES_DIRECTORY}\Fonts"
  
  CreateDirectory "${PLUGIN_DIRECTORY}"
  CreateDirectory "${PLUGIN_FILETYPES_DIRECTORY}"
  CreateDirectory "${PLUGIN_SEARCHING_DIRECTORY}"
  CreateDirectory "${RESOURCES_DIRECTORY}"
  CreateDirectory "${FONTS_DIRECTORY}"
  
  ; native filetype plugins
  SetOutPath "${PLUGIN_FILETYPES_DIRECTORY}"  
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Plugins\FileTypes\TextFile.dll"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Plugins\FileTypes\HtmlFile.dll"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Plugins\FileTypes\HtmlAgilityPack.dll" ;HtmlFile dependency
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Plugins\FileTypes\RtfFile.dll"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Plugins\FileTypes\WordDoc.dll"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Plugins\FileTypes\DocX.dll" ;WordDoc dependency
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Plugins\FileTypes\PngFile.dll"
  
  ; native search plugins
  SetOutPath "${PLUGIN_SEARCHING_DIRECTORY}"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Plugins\Searching\UltimateGuitar.dll"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Plugins\Searching\GuitartabsDotCC.dll"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Plugins\Searching\Songsterr.dll"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Plugins\Searching\HtmlAgilityPack.dll" ;common dependency
  
  ; resources
  SetOutPath "${FONTS_DIRECTORY}"
  File "${SOLUTION_DIRECTORY}\Tabster\bin\Release\Resources\Fonts\SourceCodePro-Regular.ttf"
  
  ; file association
  ${registerExtension} "$INSTDIR\Tabster.exe" ".tabster" "Tabster File"
  ${registerExtension} "$INSTDIR\Tabster.exe" ".tablist" "Tabster Playlist"
SectionEnd

Section -Post
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\Tabster.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\Tabster.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
SectionEnd

Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) was successfully removed from your computer."
FunctionEnd

Function un.onInit
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove $(^Name) and all of its components?" IDYES +2
  Abort
FunctionEnd

Section Uninstall
  Delete "$INSTDIR\Tabster.exe"
  Delete "$INSTDIR\Tabster.Core.dll"
  Delete "$INSTDIR\Tabster.Data.dll"
  Delete "$INSTDIR\Tabster.Utils.dll"
  Delete "$INSTDIR\Tabster.WinForms.dll"
  
  ; native filetype plugins
  Delete "$INSTDIR\Plugins\FileTypes\TextFile.dll"
  Delete "$INSTDIR\Plugins\FileTypes\HtmlFile.dll"
  Delete "$INSTDIR\Plugins\FileTypes\HtmlAgilityPack.dll" ;HtmlFile dependency
  Delete "$INSTDIR\Plugins\FileTypes\RtfFile.dll"
  Delete "$INSTDIR\Plugins\FileTypes\WordDoc.dll"
  Delete "$INSTDIR\Plugins\FileTypes\DocX.dll" ;WordDoc dependency
  Delete "$INSTDIR\Plugins\FileTypes\PngFile.dll"
  
  ; native search plugins
  Delete "$INSTDIR\Plugins\Searching\UltimateGuitar.dll"
  Delete "$INSTDIR\Plugins\Searching\GuitartabsDotCC.dll"
  Delete "$INSTDIR\Plugins\Searching\Songsterr.dll"
  Delete "$INSTDIR\Plugins\Searching\HtmlAgilityPack.dll" ;common dependency
  
  ; third-party references  
  Delete "$INSTDIR\ObjectListView.dll"
  Delete "$INSTDIR\System.Data.SQLite.dll"
  Delete "$INSTDIR\SQLite.Interop.dll"
  Delete "$INSTDIR\log4net.dll"
  
  Delete "$INSTDIR\Uninstall.exe"
  Delete "$DESKTOP\Tabster.lnk"
  Delete "$SMPROGRAMS\Tabster\Tabster.lnk"
  RMDir "$SMPROGRAMS\Tabster"
 
  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
  
  ; file association
  ${unregisterExtension} ".tabster" "Tabster File"
  ${unregisterExtension} ".tablist" "Tabster Playlist"
  
  SetAutoClose true
SectionEnd