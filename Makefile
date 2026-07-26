.PHONY: build
OUTPUT_DIR = ./Build/Output/

clean:
	rm -rf ${OUTPUT_DIR}/*
	find . -mindepth 2 -maxdepth 2 -type d \( -name bin -o -name obj \) -exec rm -rf {} +

publish-windows:
	rm -rf ${OUTPUT_DIR}/UnityHub_Windows
	rm -rf ${OUTPUT_DIR}/UnityHub_WindowsMSI

	dotnet publish UI/UI.csproj \
		-c Release \
		-r win-x64 \
		-p:GitVersion="$(GIT_VERSION)" \
		-p:GitSha="$(GIT_SHA)" \
		--self-contained true \
		-p:PublishSingleFile=false \
		-p:IncludeAllContentForSelfExtract=true \
		-o ${OUTPUT_DIR}/UnityHub_Windows
		
	dotnet build Installer.Wix/Installer.Wix.wixproj -c Release -p:Platform=x64 -o ${OUTPUT_DIR}/UnityHub_WindowsMSI
		
	powershell -Command "Compress-Archive -Path '$(OUTPUT_DIR)/UnityHub_Windows/*' -DestinationPath '$(OUTPUT_DIR)/UnityHub_Windows.zip' -Force"
	rm -rf ${OUTPUT_DIR}/UnityHub_Windows

publish-appimage:	
	dotnet publish UI/UI.csproj \
		-c Release \
		-r linux-x64 \
		-p:GitVersion="$(GIT_VERSION)" \
		-p:GitSha="$(GIT_SHA)" \
		--self-contained true \
		/p:PublishSingleFile=false \
		/p:IncludeAllContentForSelfExtract=true \
		-o ${OUTPUT_DIR}/UnityHub

	rm -rf ${OUTPUT_DIR}/UnityHub.appimage
	
	chmod +x ./Build/AppImageData/AppRun

	mkdir -p ${OUTPUT_DIR}/UnityHub.AppDir/usr/bin
	mkdir -p ${OUTPUT_DIR}/UnityHub.AppDir/usr/share/icons/hicolor/256x256/apps/
	
	cp -r ./Build/AppImageData/* ${OUTPUT_DIR}/UnityHub.AppDir/
	cp -r ${OUTPUT_DIR}/UnityHub/* ${OUTPUT_DIR}/UnityHub.AppDir/usr/bin
	
	cp ./Build/_Shared/Icon.png ${OUTPUT_DIR}/UnityHub.AppDir/UnityHub.png
	cp ./Build/_Shared/Icon.png ${OUTPUT_DIR}/UnityHub.AppDir/usr/share/icons/hicolor/256x256/apps/UnityHub.png
	
	ARCH=x86_64 appimagetool ${OUTPUT_DIR}/UnityHub.AppDir ${OUTPUT_DIR}/UnityHub.appimage
	chmod +x ${OUTPUT_DIR}/UnityHub.appimage