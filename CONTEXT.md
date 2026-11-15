# Wickfall - project context

2D top-down action-RPG in Unity 6000.3 (URP 2D, C#). The player is a small candle golem. His wax is both his health and his light: less wax means a smaller, dimmer light. Goal: explore a dark level, avoid or fight enemies, reach the brazier.

Required features: main menu (Play, Options, Exit), 4-direction player movement with animation, wax system (max value, decreases on damage, restored by pickups), Light 2D intensity/radius tied to wax, one enemy type (patrol, detect player, attack, can be killed), player attack, UI wax bar plus a progress counter, brazier as checkpoint that restores wax.

Rules for the agent:
- Only write or edit C# scripts in Assets/Scripts. Never modify scenes, prefabs or .meta files.
- Use Rigidbody2D.linearVelocity, not the deprecated velocity.
- Make tunable values public or [SerializeField] so I can set them in the Inspector.
- One script per task, short and commented.
- I am a student and a beginner with Unity, so after each script tell me which GameObject to attach it to and which fields to fill in.