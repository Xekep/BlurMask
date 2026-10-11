# Derived from the pinned upstream CMakeLists.txt source list and Windows defines.
# All third-party libraries are built from the pinned source archives, not from
# the upstream repository's prebuilt library directory.
SOURCE ?= ../../sources/giac-bdbb1c41fe52d8b4fb7c1d15c2deaea88fc3803c
PREFIX ?= ../../prefix
RECIPE ?= .
CXX := g++
.DEFAULT_GOAL := icas.exe
NAMES := alg_ext cocoa csturm derive desolve ezgcd freeglut_stroke_roman gauss gausspol gen global help identificateur ifactor index input_lexer input_parser intg intgab isom lin maple mathml misc modfactor modpoly moyal opengl pari permu plot plot3d prog quater risch rpn series solve sparse subst sym2poly symbolic tex threaded ti89 tinymt32 TmpFGLM TmpLESystemSolver unary usual vecteur
OBJECTS := $(addsuffix .o,$(NAMES)) icas.o
CPPFLAGS := -I$(PREFIX)/include -I$(SOURCE)/src/giac/headers -DHAVE_NO_HOME_DIRECTORY -DGIAC_GGB -DIN_GIAC -DHAVE_LIB_PTHREAD -DGIAC_GENERIC_CONSTANTS -DTIMEOUT -DSIZEOF_VOID_P=8 -DHAVE_LIBMPFR -DVERSION=\"bdbb1c41\" -DGIAC_MPQS -D__MINGW_H -DMINGW32 -DHAVE_NO_SYS_TIMES_H -DHAVE_NO_SYS_RESOURCE_WAIT_H -DHAVE_NO_PWD_H -DHAVE_NO_CWD -DNO_CLOCK -Dusleep= -DYY_NO_UNISTD_H
CXXFLAGS := -O2 -std=c++11 -fpermissive -fno-strict-aliasing -ffunction-sections -fdata-sections -w
VPATH := $(SOURCE)/src/giac/cpp
$(OBJECTS): $(RECIPE)/Makefile $(SOURCE)/src/giac/headers/gmp.h
icas.exe: $(OBJECTS)
	$(CXX) -static -static-libgcc -static-libstdc++ -Wl,--gc-sections,--no-insert-timestamp -s -o $@ $(OBJECTS) $(PREFIX)/lib/libmpfr.a $(PREFIX)/lib/libgmp.a -lwinpthread -lm
%.o: %.cc
	$(CXX) $(CPPFLAGS) $(CXXFLAGS) -c $< -o $@
%.o: %.cpp
	$(CXX) $(CPPFLAGS) $(CXXFLAGS) -c $< -o $@
%.o: %.c
	$(CXX) $(CPPFLAGS) $(CXXFLAGS) -c $< -o $@
icas.o: $(RECIPE)/icas.cpp
	$(CXX) $(CPPFLAGS) $(CXXFLAGS) -c $< -o $@
