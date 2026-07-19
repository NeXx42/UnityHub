OUTPUT_DIR = ./Build/Output/

build:
	rm -rf ${OUTPUT_DIR}/*

	# app
	dotnet publish UI/UI.csproj \
		-c Release \
		-r linux-x64 \
		--self-contained true \
		/p:PublishSingleFile=false \
		/p:IncludeAllContentForSelfExtract=true \
		-o ${OUTPUT_DIR}/UnityHub

build-appimage:	
	chmod +x ./Build/AppImageData/AppRun

	mkdir -p ${OUTPUT_DIR}/UnityHub.AppDir/usr/bin
	mkdir -p ${OUTPUT_DIR}/UnityHub.AppDir/usr/share/icons/hicolor/256x256/apps/
	
	cp -r ./Build/AppImageData/* ${OUTPUT_DIR}/UnityHub.AppDir/
	cp -r ${OUTPUT_DIR}/UnityHub/* ${OUTPUT_DIR}/UnityHub.AppDir/usr/bin
	
	cp ./Build/_Shared/Icon.png ${OUTPUT_DIR}/UnityHub.AppDir/UnityHub.png
	cp ./Build/_Shared/Icon.png ${OUTPUT_DIR}/UnityHub.AppDir/usr/share/icons/hicolor/256x256/apps/UnityHub.png
	
	ARCH=x86_64 appimagetool ${OUTPUT_DIR}/UnityHub.AppDir ${OUTPUT_DIR}/UnityHub.appimage
	chmod +x ${OUTPUT_DIR}/UnityHub.appimage