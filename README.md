# Endless kiwi runner
![Kiwi runner screenshot](https://github.com/med-material/bci-kiwi-runner-redux/blob/main/kiwi-runner.png)

## About
This game was made as a semester project on the 2nd semester of Medialogy MSc in Spring ’19. The game is intended for motivating rehabilitation of stroke patients through the use of BCI with fabricated feedback.

## Contributors
The game was created at Aalborg University by Ingeborg Goll Rossau, Rasmus Bugge Skammelsen, Jedrzej Jacek Czapla and Mozes Adorjan Miko, with additional scripts for managing input and logging added later, created by Bastian Ilsø Hougaard. All sound effects are licensed under Creative Commons.

## Game
In the game, a kiwi runs across a stretch of land to save its babies from an eagle. On the way, it encounters various obstacles. To get past the obstacles without slowing down, the kiwi must jump on the trampoline, which lands it in an aircurrent which carries it across the obstacle. Occasionally when the player fails to make the kiwi jump, fabricated feedback will be activated, making the kiwi jump anyway.

## Input
The project has three options for input: blinking, key sequence and BCI. To switch between them, activate the appropriate script on the InputManager object in the editor. The kiwi can only jump when within the input window (on the trampoline).
*	BCI: the player makes the kiwi jump by performing motor imagery. Requires BCI hardware or a simulated signal.
*	Blinking: the player makes the kiwi jump by blinking. The kiwi will jump when the player closes their eyes within the input window. Requires a Tobii eye tracker.
*	KeySequence: the player makes the kiwi jump by typing in a keysequence in the correct order within a time limit. The time limit can be adjusted (Sequence Time Limit), as can the sequence (Keyboard Sequence). Useful for internal testing.

## Game settings
To set up the game for experiments, there are a number of variables that can be adjusted. All can be found on the GameManager in the Unity editor:
*	Game setup
    *	Game Speed: Adjusts the speed of the environment, range 0-10. Recommended value: between 7-10. This affects the time it takes to walk through an obstacle when failing to jump as well as the time between exiting the previous obstacle and reaching the cue (signpost) for the next one (mix of cue and ITI). At speed = 0, this time is ~9 seconds. At speed = 10, this time is ~4.5 seconds.
    *	Trampoline Color: To help players differentiate between conditions, you can change the color of the trampoline.
*	Trial setup
    *	Rej Trials: How many trials should (at minimum) result in negative feedback (i.e. kiwi doesn’t jump).
    *	Acc Trials: How many trials should (at maximum) result in positive feedback (i.e. kiwi jumps)
    *	Fab Trials: How many trials should reject user input and fire fabricated input (i.e. kiwi jumps at random time within input window).
*	FabInput Settings
    *	No Input Received Fab Alarm: The average time at which fabricated input fires.
    *	Fab Alarm Variability: How much the fabricated input should vary.
*	Paradigm Settings
   *	Input Window Seconds: How long the time in which players can make an input is. Adjusts the size of the trampoline.
    *	Prep Phase Seconds: How long the time between reaching the cue (signpost) and entering the trampoline is. Adjusts the distance between cue and trampoline.

## Logging
When the game ends (or OnApplicationQuit), the game saves two logs in the /documents folder (unless otherwise specified): Game and Meta. The Meta log saves information about the settings that can be adjusted via the GameManager. The Game log saves information about events throughout the game:
*	Input window status (closed/open)
*	Inter-trial interval length (from end of feedback to start of prep phase)
*	Input window length (from beginning of trampoline till feedback)
*	Game state (running/stopped)
*	Current fab alarm (when fabricated input fires, if applicable)
*	No. of trials left + rate of acc/rej/fab input
*	Trial result
*	Trial goal
For more information about the LoggingManager, see: https://github.com/med-material/LoggingManager.
