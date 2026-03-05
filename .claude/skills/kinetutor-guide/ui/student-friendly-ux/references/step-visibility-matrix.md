# Step Visibility Matrix

| Step | Left | Right | Bottom | Focus |
|---|---|---|---|---|
| S1 | DHTable | Hidden | Hidden | DHTable |
| S2 | Hidden | FrameInfoOverlay | Hidden | Viewport3D |
| S3 | FourMatrices | Hidden | Slider(1) | MatrixPanel |
| S4 | MultiplicationProgress | Hidden | Hidden | MatrixPanel |
| S5 | DHReference | AiColorCoding | Slider(1) | RightPanel |
| S6 | CumulativeProduct | A1A2Reference | Slider(2) | Viewport3D |
| S7 | T0nAndExtract | PoseExtract | Slider(2) | EndEffectorFrame |
| S8 | FullDH | FullMatrices | AllSliders | None |

## Animation Tokens
- Panel Slide In: 400ms / EaseOutCubic
- Panel Fade Out: 250ms / EaseInQuad
- Focus Pulse: 1200ms / PingPong
- Tooltip Pop In: 200ms / EaseOutBack
