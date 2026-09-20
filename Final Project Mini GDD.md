# Game Design Document (GDD)

**Projektname:** Sector Mortis

**Gruppenmitglieder:** Maximilian Effmert, Anton Filimonov, Bassem Al-Barudi & Tim Klarner

**Genre:** Wave-Based Survival First-Person Shooter (FPS) / Action

**Plattform:** PC (Windows)

---
## **1. Spielidee / Ziel / Scope**
- **Spielidee:**
	- Ein rundenbasierter Zombie-Survival-Shooter, der das klassische Gameplay von *Call of Duty: Black Ops II Zombies* (Mob of the Dead) mit dem prozeduralen Waffensystem von *Borderlands* kombiniert.
- **Szenario:**
	- Eine düstere, isolierte Insel mit einer zentralen Gefängnisanlage (angelehnt an Alcatraz).
- **Ziel des Spiels:**
	- Der Spieler versucht, so viele entgegenströmende Zombiewellen wie möglich zu überleben. Es gibt kein finales Ende oder Siegesbedingung (Überleben bis zum Tod).
- **Highscore-System:**
	- Die Leistung wird am Ende durch das **insgesamt erwirtschaftete Geld** (Score) gemessen, ohne dass getätigte Ausgaben abgezogen werden. So bleibt der Anreiz hoch, Geld auch während des Spiels auszugeben.
- **Scope:**
	- Singleplayer auf einer fokussierten Karte mit Türen-Freischaltsystem, modularem Waffen-Generator, Kauf-Shops/Mystery-Box und Perk-Stationen.

---
## **2. Kernmechanik / Spielablauf**
### **2.1 Spielablauf (Game Loop)**
- **Spawn:**
	- Der Spieler startet mit einer Basispistole in einem abgegrenzten Areal.
- **Wellen-System:**
	- Zombies erscheinen in aufeinanderfolgenden, kontinuierlich schwerer werdenden Wellen.
	- Die Schwierigkeit der Wellen wird durch eine Kombination aus steigender Anzahl und Robustheit der Zombies stetig erhöht.
- **Ökonomie:**
	- Durch Treffer, Kills und das Reparieren von Barrikaden verdient der Spieler Geld.
	- Kopftreffer geben doppelte Basistrefferpunkte.
- **Progression:**
	- Mit Geld können Türen freigekauft werden, wodurch neue Kartenbereiche zugänglich werden.
	- Kauf neuer zufälliger Waffen über die Mystery-Box oder aus einem rotierenden 3er-Shop-Angebot.
	- Freischalten passiver Buffs/Perks an verteilten Interaktionspunkten.
- **Game Over:**
	- Der Durchlauf endet mit dem Tod des Spielers. Nach dem Tod werden die Statistiken des Durchlaufs angezeigt (Punkte, Kills, Tode).

### **2.2 Kernmechaniken & Systeme**
- **Prozedurales Waffensystem (Borderlands-Prinzip):**
	- Waffen werden nicht als feste Assets geladen, sondern aus variablen Einzelkomponenten (z. B. Lauf, Schaft, Magazin, Visier, Gehäuse) zufällig kombiniert.
	- Dies erzeugt eine extrem hohe Kombinationsvielfalt mit unterschiedlichen Statuswerten (Schaden, Feuerrate, Magazingröße, Präzision).
- **Waffen-Beschaffung:**
	- **Mystery-Box:** Liefert gegen ein festes Entgelt eine komplett zufallsgenerierte Waffe. Es besteht eine Chance keine Waffe zu erhalten, wohingegen die Mystery-Box im Anschluss an einer neue Position platziert wird.
	- **Shop:** Bietet eine Auswahl von 3 Waffen zum direkten Kauf an. Jede Welle aktualisiert sich die Auswahl, welche zusätzlich auch immer gegen Geld neu gewürfelt werden kann.
- **Perk-System:**
	- In der Spielwelt verteilte Objekte (Zigarettenautomaten) fungieren als Perk-Stationen. Mit diesen kann man interagieren, um den Perk dieses Objektes zu erhalten.
	- **Perks:**
		- Health: 200 Lebenspunkte anstatt 100, was den Spieler deutlich mehr Zombie Attacken hintereinander überleben lässt.
		- Revive: Wenn der Spieler stirbt, wird er einmalig nach einem 10 Sekunden Zeitlupenfenster wiederbelebt und kann das Spiel fortsetzen.
		- Stamina: 200 Ausdauer anstatt 100, womit der Spieler öfter sprinten und springen kann, bevor er sich wieder kurz ausruhen muss.
		- Dritte Hand: Der Spieler kann eine dritte Waffe besitzen (zwei Waffenplätze sind der Standardwert).
- **Map-Gating & Barrikaden:**
	- Türen können mit Geld freigekauft werden. Dadurch lassen sich neue Areale der Karte freischalten in welchen sich möglicherweise neue Inhalte (z.B. Perks) befinden.
	- Barrikaden an Fenstern bremsen den Zombiestrom aus dem dahinterliegenden Gebiet und dienen als passive Punktequelle durch Reparatur.
- **Zombies:**
	- Die Körperteile von Zombies haben eigenständige Schadenserkennung und können durch Beschädigung abgetrennt werden.
	- Zombies mit weniger als zwei Beinen kriechen auf dem Boden.
- **Menü und Einstellungen:**
	- Das Hauptmenü hat einen bewegten Hintergrund, welcher zum Spielszenario passt.
	- Über die Einstellungen lassen sich Grafikeinstellungen anpassen.
	- Andere Einstellungen wie Audioeinstellungen und Tastenbelegung lassen sich ebenfalls anpassen.

### **2.3 Steuerung**
- Anmerkung: Tasten können in den Einstellungen umgelegt werden.
- Bewegung:
	- Der Spieler kann sich mit einer klassischen "WASD"-Steuerung bewegen und mit der "Leertaste" springen.
- Blickrichtung ändern:
	- Die Blickrichtung kann durch bewegen der Maus verändert werden.
- Interaktion:
	- Mit der Taste "e" kann mit bestimmten Gegenständen (z.B. Türen, Zigarettenautomaten, Händler) interagiert werden, wenn man nah genug am Gegenstand steht und diesen anschaut.
- Waffensystem:
	- Mit der linken Maustaste kann man schießen und mit der rechten Maustaste anvisieren.
	- Mit der Taste "r" kann man nachladen.
	- Mit dem Mausrad kann man zwischen den einzelnen Waffenslots wechseln, solange diese mit einer Waffe belegt sind.
- Menüs:
	- Die Menüs lassen sich mit der Maus bedienen.

---
### **3. Assets / Abgabe / Quellen**
- **Abgabe-Umfang:**
	- Ausführbare Build-Datei (`.exe`).
	- Trailer.
	- Quellcode / Projekt-Repository.
	- Diese Dokumentation (GDD).
- **In Eigenarbeit erstellte Assets:**
	- Modulare Waffen-3D-Bausteine (Läufe, Visiere, Body, usw.).
	- Verknüpfung zwischen Zombie Asset und Animationen.
	- Kartengestaltung mit vorgefertigten Assets.
- **Audio-Assets:**
	- https://freesound.org/
- **Material Quellen:**
	- https://polyhaven.com/textures
	- https://freepbr.com/
- **3D Asset Quellen & Lizenzen:**
	- *"Burn Barrels"* von Caboose3d: https://sketchfab.com/3d-models/burn-barrels-bd66fe8403d147ceac9d02d8aa18afdc (CC Attribution)
	- *"Modular Prison Asset Pack"* von Marika Cavecchia: https://www.artstation.com/marketplace/p/AJkJa/blender-unity-urp-modular-prison-asset-pack (gekauft)
	- *"Zombie"* von pxltiger: https://sketchfab.com/3d-models/zombie-73ef58af341e46afba1da53366ed79cf (CC Attribution)
	- *"Rusty Folding Chairs"* von Oliver Triplett: https://sketchfab.com/3d-models/rusty-folding-chairs-636b1ae89d9449b8bcd670adbb7eb173 (CC Attribution)
	- *"Fire Extinguisher Old Rusty 3D Scan"* von grafi: https://sketchfab.com/3d-models/fire-extinguisher-old-rusty-3d-scan-70a4fdec06c64372ba3a96ae49257ce1 (CC Attribution)
	- *"Rusty Old Newspaper Vending Machine"* von chrisg4919: https://sketchfab.com/3d-models/rusty-old-newspaper-vending-machine-29cca5a8e99e438c8fb644d1aea3587e (CC Attribution)
	- *"Rusty Industrial Wardrobe"* von Nikoleta.Zhecheva: https://sketchfab.com/3d-models/rusty-industrial-wardrobe-4543aa4f542c46c3945c22568423cade (CC Attribution)
	- *"Rusty Metal Barrel Prop"* von Wille Evensson: https://sketchfab.com/3d-models/rusty-metal-barrel-prop-df53c334113e44258e306a185ae86aee (CC Attribution - Non Commercial)
	- *"Old Medicine Cabinet"* von James Penman: https://sketchfab.com/3d-models/old-medicine-cabinet-9a092f5d0f3a4373a21fe30bafbe5488 (CC Attribution)
	- *"Aluminium mug"* von Mikhail Antonov: https://sketchfab.com/3d-models/aluminum-mug-7bc2b14a48d1413e9375c32a53e3ee6f (CC Attribution)
	- *"Power transformer Building roof top Electricity"* von Mehdi Shahsavan: https://sketchfab.com/3d-models/power-transformer-building-roof-top-electricity-2ac5068cf01649fa8d9d9e8879b32710 (CC Attribution)
	- *"Bowl"* von Melon Polygons: https://sketchfab.com/3d-models/bowl-dd50cb0550ab4d64b83820009407fdec (CC Attribution)
	- *"Bucket"* von Melon Polygons: https://sketchfab.com/3d-models/bucket-4bca97591fa24b699d056890e76f147f (CC Attribution)
	- *"Glass Bottles"* von FrodoUndead: https://sketchfab.com/3d-models/glass-bottles-334377879cb4475d9a4720a2f7c4cf55 (CC Attribution)
	- *"Outside Electrical box"* von eXtrime95: https://sketchfab.com/3d-models/outside-electrical-box-60cf48a4d4f645d999bcecede7ca9113 (CC Attribution)
	- *"Wall Clock"* von FelipeMSX: https://sketchfab.com/3d-models/wall-clock-2e964ac0242e4b1789adfd9549c653dc (CC Attribution)
	- *"Old Rusty Radiator - Game Ready Asset"* von Gravity3D: https://sketchfab.com/3d-models/old-rusty-radiator-game-ready-asset-4a900e41cf4748eaafe77977a2d1ffa3 (CC Attribution)
	- *"Rusty Fence Part 1"* von \_dark\_alice\_: https://sketchfab.com/3d-models/rusty-fence-part-1-cb99da9c518d4000a8a688ffb49b0b81 (CC Attribution)
	- *"Dumpster"* von txnomod: https://sketchfab.com/3d-models/dumpster-337b3a4424b745699cda776545070e04 (CC Attribution)
	- *"Modular Airduct Rectangular 01 4k"* von mohamedhussien: https://sketchfab.com/3d-models/modular-airduct-rectangular-01-4k-af366ffad3bf4ae7ad9309158f5ebec0 (CC Attribution)
	- *"Trash can"* von LiliumLetifer: https://sketchfab.com/3d-models/trash-can-e63f57d1f72140e4b37c25880677449d (CC Attribution)
	- *"Lamp 22"* von Console Art Cybernetic: https://sketchfab.com/3d-models/lamp-22-9200262ca1cd47b4bb139c8c150b4231 (Free Standard)
	- *"Barrels and pallet"* von George Hager: https://sketchfab.com/3d-models/barrels-and-pallet-da8bc019be634479a121aede1a47faab (CC Attribution)
	- *"Old Bloody Wood Planks"* von DavisCaveArt3d: https://sketchfab.com/3d-models/old-bloody-wood-planks-d5502623455446c3983d2eed057151cd (CC Attribution)
	- *"Light Ceiling Lamp - 15MB"* von Mehdi Shahsavan: https://sketchfab.com/3d-models/light-ceiling-lamp-15mb-eaaab08eac454e59840fa4818fc55ff3 (CC Attribution)
	- *"Zombie Animations Set"* von RamsterZ: https://assetstore.unity.com/packages/3d/animations/zombie-animations-set-220537 (gekauft)
	- *"Free Quick Effects Vol.1 - Asset Store Showcase"* von Gabriel Aguiar Prod: https://assetstore.unity.com/packages/vfx/particles/free-quick-effects-vol-1-304424 (Free Standard)
	- *"Survivalist character"* von Slayver: https://assetstore.unity.com/packages/3d/characters/survivalist-character-181470 (Free Standard)
	- *"german Cigarette machine"* von The Latest Shit: https://sketchfab.com/3d-models/german-cigarette-machine-07d5c355c88a4583a3c965422937da96 (CC Attribution)
	- *"Dj Khaled"* von Mr. PooPoo: https://sketchfab.com/3d-models/dj-khaled-bda3f0e290db4de5b9ed3a9a22e44499 (CC Attribution)
	- *"Fat Joint"* von 3D Crackhouse: https://sketchfab.com/3d-models/fat-joint-5b7dcea4012d4843a4c2baac027c7e78 (CC Attribution)

