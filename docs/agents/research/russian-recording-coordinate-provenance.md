# Russian recording coordinates and missing stroke boundaries

The existing `y = -originalY` conversion is supported by the recorder, rather
than selected by recognition accuracy. The pinned
[collection program](https://github.com/Skvayzer/russian_handwritings_tracked/blob/c6629026b1fdef51fd08e3d2fddffd82f97e60a5/scanner.py)
uses pyglet's default window/OpenGL coordinate system.
[Pyglet documents](https://docs.pyglet.org/en/latest/programming_guide/mouse.html)
an origin at the lower left, with Y increasing upwards. Negating Y gives the
downward coordinate direction used by the application's ink. The recorder's
guide levels200/250 consequently become−200/−250; the loader does not supply
those guides to recognition. No alternate reflection was fitted to word targets.

The recorder appends `x+dx,y+dy` during accepted drag events, with a minimum10ms
sampling interval. Its release handler does nothing. It clears and saves the
point list per completed sample, without storing stroke separators. A single
CSV coordinate sequence therefore does **not** establish that the writer never
lifted the pen: multiple physical strokes may have been concatenated. The loader
preserves the supplied sequence as one input stroke because exact original
boundaries are unavailable. It must not invent releases from gaps or timing.
These are static observations of the recorder; it was not executed.

The [dataset description](https://data.mendeley.com/datasets/3h6h5d7xg2/2)
also identifies XY/timing as the recorded channels and describes the PNG as the
writer-visible image. The coordinate and boundary limitations constrain the
recorded-word diagnostics. They neither prove that missing boundaries caused
the errors nor qualify current Russian recognition as usable. Existing input
coordinates, outputs and accuracy totals are unchanged.
