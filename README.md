###### DESCRIPTION

A 2D football game made in XNA for a Game Development course in the Integrated Master of Informatics and Computing Engineering degree from the University of Porto. The development team was composed of:

João Xavier (jcxavier in github)

Ricardo Moutinho

Rui Campos (catdawg in github)

Tiago Carvalho


Download link for the compiled game: https://dl.dropbox.com/u/16643844/FES2010.zip
You might need to install the XNA 3.1 runtime: http://www.microsoft.com/en-us/download/details.aspx?id=15163

In order to compile the game, you might need Visual C# express 2008: http://www.microsoft.com/visualstudio/en-us/products/2008-editions/express

and the XNA 3.1 game studio: http://www.microsoft.com/en-us/download/details.aspx?id=39


Development timeframe was around 3 weeks.

Major features are:

- Customizable teams, each player has specific stats such as, speed, stamina or shot power. The stats and formation can be edited in a .txt file.
- The stats affect the players by giving them less accuracy when they pass or shoot, or making them run slower.
- Sprinting depletes two kinds of stamina, one permanent and a smaller one which replenishes very fast after a short time. While any of the bars is empty, the player cannot sprint.
- Cool AI! the players will defend intelligently, and the attackers will make sprints to escape man to man marking. If you can beat the AI you are awesome :D (I can't :( ).
- If you beat the AI by 3 goal difference you are in for an auditory surprise :D


###### CONTROLS

If you wish to edit the controls yourself you will unfortunately have to compile the game yourself, the controls are present in the Controller.cs file.

Current controls are:

WARNING: some keyboards don't allow many keys to be pressed at the same time. Sorry if sometimes it might seem it isn't shooting or something, specially if you are playing Player VS Player.

Player1:

    Up = W;
    Down = S;
    Left = A;
    Right = D;
    Sprint = LeftShift;
    ChangePlayer = Q;
    Pause = D1;
    Pressure = Space;
    GoalerOut = E;
    Pass = C;
    FreePass = E;
    Shoot = X;

Player2:

    Up = I;
    Down = K;
    Left = J;
    Right = L;
    Sprint = N;
    ChangePlayer = U;
    Pause = Enter;
    Pressure = P;
    GoalerOut = E;
    Pass = OemPeriod;
    FreePass = O;
    Shoot = OemComma;

###### LICENSE

GNU GENERAL PUBLIC LICENSE

Version 3, 29 June 2007

(http://www.gnu.org/copyleft/gpl.html)