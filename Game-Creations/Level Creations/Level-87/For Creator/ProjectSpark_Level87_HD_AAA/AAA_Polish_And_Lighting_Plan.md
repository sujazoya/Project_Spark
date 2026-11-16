# Level 87 AAA-inspired art finishing plan

**Lighting**: physically based broad orbital rim highlights; gold warm coil details vs cyan bearing status; strong figure/ground separation between rotating ring and fixed stator. Keep Earth/star backdrop purely ornamental, not a lighting authority.

**Mesh pass**: production bevel normals, angle-weighted smoothing, high-poly bake on bearing stators and copper windings, material IDs per machine, unique nonrepeating fastener detail, baked trims, hand-crafted assembly decals, close-up readable terminal icons and fault labels.

**Materials**: titanium bearing race (.22 roughness), ceramic pressure hull (.33), copper windings (.24), anodized orange-gold (~.25), coated composite rotor (.61), cyan emissive magnetic status; transparent instrument glass with depth sorting and render pipeline adaptation. Procedural textures supplied are reference maps, not applied professional UV-mapped texture atlases.

**VFX**: animated cyan field arcs, amp-current status emission, rotation guide ticks, synchronized vib-spectra, severe overheat amber/red indicators, controlled touchdown warnings and W137 critical trust-loss display; only driven by authoritative simulation measurements.

**Performance**: segment main ring into static/rotating batching groups; avoid static batching moving rotor art; indirect instancing/merged mesh chunking for repeating radiator tiles and copper coils; LOD0/1/2 for 8 bearing stations and 6 habitat segment panels; occlusion zones for the room; simplified selection/collision geometry; assess triangle/draw-call/memory under target hardware.
