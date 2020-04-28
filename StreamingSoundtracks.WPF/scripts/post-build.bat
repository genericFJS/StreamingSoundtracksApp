IF "%1"=="x64" (
	SET del_dir="win-x86"
	SET clean_dir="win-x64"
) ELSE IF "%1"=="x86" (
	SET del_dir="win-x64"
	SET clean_dir="win-x86"
) ELSE (
	goto :eof
)
cd libvlc
REM delete lib of other platform
del /s /q %del_dir% > NUL
rmdir /s /q %del_dir%

REM delete unnecessary files
cd %clean_dir%
del /s /q locale > NUL
rmdir /s /q locale

cd plugins
REM delete whole plugin folders:
FOR %%f IN (
	"access_output"
	"audio_mixer"
	"codec"
	"control"
	"d3d9"
	"d3d11"
	"gui"
	"keystore"
	"lua"
	"meta_engine"
	"misc"	
	"mux"
	"services_discovery"
	"spu"
	"stream_extractor"
	"stream_filter"
	"stream_out"
	"text_renderer"
	"video_chroma"
	"video_filter"
	"video_output"
	"video_splitter"
	"visualization"
) DO (
	del /s /q %%f > NUL
	rmdir /s /q %%f
)

REM delete all plugins in some remaining folders except a few
cd "access"
FOR %%i IN (*) DO (
	IF "%%i" == "libhttp_plugin.dll" (
		echo ""
	) ELSE IF "%%i" == "libhttps_plugin.dll" (
		echo
	) ELSE (
		del /s /q %%i > NUL
	)
)
cd ..

cd "audio_filter"
FOR %%i IN (*) DO (
	IF "%%i" == "libmad_plugin.dll" (
		echo
	) ELSE (
		del /s /q %%i > NUL
	)
)
cd ..

cd "audio_output"
FOR %%i IN (*) DO (
	IF "%%i" == "libwaveout_plugin.dll" (
		echo
	) ELSE (
		del /s /q %%i > NUL
	)
)
cd ..

cd "demux"
FOR %%i IN (*) DO (
	IF "%%i" == "libes_plugin.dll" (
		echo
	) ELSE (
		del /s /q %%i > NUL
	)
)
cd ..

cd "packetizer"
FOR %%i IN (*) DO (
	IF "%%i" == "libpacketizer_mpegaudio_plugin.dll" (
		echo
	) ELSE (
		del /s /q %%i > NUL
	)
)
cd ..