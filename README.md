# Friendly-Fired
A mod that makes it so that used abilities cant harm its owner (the person who used the ability). (missile, mine, smoke, grenade, tesla coil, spike, arrow, black hole). The Black hole no longer kills you or pulls you towards it

*Disables the hurtbox and 'pullbox' (black holes) of certain abilities for the owner of that ability*

## Quick Links
* **[MyBoplMods Repo](https://github.com/maxgamertyper/MyBoplMods)**
* **[Youtube Video](https://youtu.be/HXrcDflS_Wg)**
* **[Direct Video Download](https://github.com/maxgamertyper/Friendly-Fired/blob/main/FriendlyFired.mp4)**
* **[Thunderstore Link](https://thunderstore.io/c/bopl-battle/p/maxgamertyper1/FriendlyFired/)**

> *The Youtube video doesn't include the full capabilites of the mod as that video was recorded with V1.0.0*

---

## General Information & Setup

### Mod-Manager Setup
> *this is specifically guided for thunderstore, it may be slightly different for other mod managers*

#### Prerequisites
* A mod-manager (thunderstore, R2modman, or others) configured for the game bopl battle
* The game Bopl Battle

#### Steps

1) access the **Bopl Battle** game
2) make a new mod profile
3) go to the mods tab
4) search for "FriendlyFired"
5) click download
6) run the game twice (it won't work the first time as the manager is initializing the mod installer)
7) check your mod manager's config area to change the different abilities that harm you
8) have fun


### BepInEx Setup
> *note: this is directed towards a windows installation*

#### Prerequisites
* An installation of the BepInEx Zip file
* the game Bopl Battle
* the FriendlyFired.dll file

#### Steps
1) find your game directory through steam likely at `C:\Program Files (x86)\Steam\steamapps\common\Bopl Battle`
2) unzip the BepInEx file into the folder
3) run the game once
4) return to the directory
5) move the FriendlyFired.dll file into the plugins folder
6) run the game
7) check the BepInEx config directory for the config file
8) change the different abilities that harm you
9) have fun

---

## Configuration Architecture

This mod has multiple different configuration options:
> *When a config setting is set to true, that means that specific ability will be patched, it will **not** kill the player who fired it. If it is set to false, default game behavior will be applied and it **will** kill the player who fired it*

* **Explosion Patches**
    * **Missile Patch:** Controls if your own missile explosions inflict self-damage.
    * **General Explosion Patch:** Controls if your Grenade, Smoke Grenade, and Mine explosions inflict self-damage.
* **Physical Patches**
    * **Tesla Coil Patch:** Toggles self-damage for your placed tesla coils.
    * **Spike Patch:** Toggles self-damage for your own spike or sword.
* **Other Patches**
    * **Arrow Patch:** Grants invulnerability to your own arrows.
    * **Black Hole Patch:** Disables the black hole's gravity pull and killbox exclusively for the owner.
