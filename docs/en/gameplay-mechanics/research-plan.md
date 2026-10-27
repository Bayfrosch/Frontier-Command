# Research plan

Status: Design in progress. This document records the planning discussion through October 18, 2026. The research overlay section describes the current outline implementation. Other sections describe intended gameplay.

Agreed decisions, tentative ideas, and open questions are identified separately. Older faction descriptions are orientation material. Where this plan differs from those descriptions, this plan records the current direction.

## Scope and faction identity

The first release focuses on one human faction. The other two factions follow after the game is finished.

The human faction is the basic, balanced faction. It is beginner friendly, has no extreme strengths or weaknesses, and does not specialize in a narrow mechanic before choosing a doctrine. Its final name is undecided. The existing Frontier Coalition name and faction descriptions are provisional.

The long-term plan remains three factions, each with two mutually exclusive doctrines. Each doctrine effectively creates a subfaction with a distinct way of fighting.

## Purpose of research

Research controls most progression. It replaces building dependency chains as the main way to unlock advanced content. A small number of infrastructure prerequisites may remain.

Research primarily unlocks units and buildings. It also provides passive improvements, including damage bonuses, and a few abilities.

Most ordinary unit abilities belong to the unit when it becomes available. Capital units gain their abilities primarily through battle experience and rank.

Research competes with army production and expansion for Materials. Players can concentrate on a path and reach its stronger technologies sooner, or invest broadly and delay their strongest options.

## Starting access

The following toolkit is available without research. These are construction and production options, not a list of everything granted at match start:

- Command structure.
- Construction units.
- Supply center or refinery and harvesters.
- Basic generators.
- Barracks.
- Scouts.
- Basic soldiers.
- Envoys for capturing outposts.
- Research center, subject to its possible supply-center prerequisite.

The term "capture units" is understood to mean Envoys. The exact starting soldier roster and access to basic defenses still need definition.

A tentative exception to research-led progression is to require a completed supply center before constructing the research center. The developer handbook calls the existing `RESOURCE_GATHERER` building the Supply Center. The final terminology and prerequisite remain open.

At match start, the player also chooses one capital unit from a small selection and receives that unit. The choices and their relationship to capital production research remain open.

## Core tree and doctrine tree

Research has two main parts. These parts are distinct from technology generations, whose number is not yet defined.

### Core tree

The core establishes the human faction's foundation. Choices within the core have a smaller impact than the later doctrine choice.

Before the doctrine split, the faction has access through research to a basic version of every necessary battlefield role. Examples include tanks and anti-air units. The complete roster still needs definition.

Early progression is fast enough to unlock starting technologies, such as basic tanks, without a long delay.

The core trees lead into the doctrine decision. The player must complete any two Military core trees and any one Civilian core tree. Completing a tree means researching every technology in its first generation. Fleet trees do not count toward this requirement.

The player can leave the remaining core branches unfinished and continue them later. Science-outpost generation advancement is a separate requirement planned for a future outpost ability.

### Doctrine tree

Choosing one doctrine permanently excludes the other for that match. The choice has a strong effect on how the faction feels and works.

Both doctrines build on the basic roster. A doctrine can improve selected core units or unlock additional, stronger, or more specialized variants.

The intended split is infantry versus tanks. These are working descriptions, not final doctrine names.

| Doctrine | Intended identity |
| --- | --- |
| Infantry | Numerous, cheaper infantry units, with strong infantry options beyond basic mass production. |
| Tanks | More expensive armored units that are generally stronger individually. |

The shared core provides answers to different threats. The exact exclusive units, upgrades, buildings, and weaknesses of each doctrine still need definition.

## Technology generations and science outposts

Technology generations provide a progression structure inspired by Age of Empires. After meeting requirements, a player upgrades to the next generation through an ability on a science outpost. Generation advancement is not a technology purchased from the research screen.

Horizontal lines separate generation tiers in each research chapter. Generation labels sit in a side gutter beside the continuous trees. Generation descriptions are omitted from the screen. The current outline shows three generations. The outpost upgrade ability and its generation restrictions will be implemented later.

Science outposts come from captured outposts that the player specializes for research. They support the research center and unlock further progress. This gives players a reason to leave their starting bases and contest territory.

Core research starts before the player owns a science outpost. Progression to a doctrine and beyond requires science-outpost access.

The following generation details remain open:

- Number and names of generations.
- Requirements, Material costs, power requirements, and duration of generation upgrades.
- Which generation enables the doctrine choice.
- Whether later generations require additional science outposts.
- Whether science is a separate specialization or part of the existing energy specialization.
- How an outpost generation upgrade pauses or resumes if the outpost is lost.

Two science outposts may also boost research speed. This is a tentative idea. The threshold, amount, and effect of further outposts are undecided.

### Losing a science outpost

The agreed behavior is:

- Completed research remains effective.
- Units unlocked by completed research remain producible.
- Active projects pause if the player no longer meets their outpost requirements.
- Paused projects retain their progress and can resume when the requirements are restored.
- Basic research that does not require an outpost remains available.

The detailed relationship between an already reached generation and continued outpost ownership still needs definition. Losing territory must remain relevant without removing completed technologies.

## Research center and chapters

Each player can have only one research center. Science outposts support that center rather than acting as additional research centers.

Research follows the category model referenced from Sins of a Solar Empire: Rebellion. The screen separates research into three chapters. Only one project per chapter can be active at a time. Different chapters can run projects simultaneously through the same center.

| Chapter | Outline trees |
| --- | --- |
| Military | Infantry, Tanks, and Aircraft. Infantry and Tanks contain the later doctrine choices. |
| Civilian | Extraction, Energy, and Infrastructure. Includes improved extractors, generators, construction, and repairs. |
| Fleet | Fleet capacity and Capital capacity. Includes capital production and capacity upgrades. |

Extra outposts do not have an agreed role in adding project slots. Their confirmed purpose is progression access, with a possible research-speed bonus.

Research costs Materials when the player clicks Start Research after selecting a technology. There is no separate confirmation dialog. Cancelling an unfinished project refunds the full paid cost. Queue length and final research-center destruction or rebuilding rules remain open.

## Material costs and Energy

Research costs Materials. Active research also consumes additional Energy capacity.

Energy follows the existing fixed-capacity model. It is power supplied by infrastructure, rather than an accumulated currency spent on a technology.

The additional research power requirement applies while a project is active. The amount per project or category remains undecided, as does the treatment of paused projects.

The following costs and rules still need definition:

- Material costs and duration of individual technologies.
- The research center's normal power requirement.
- Additional power required by active projects.
- Behavior during insufficient power, including which projects pause.
- Final Material costs and balance values. Payment when starting research and full cancellation refunds are agreed.

## Capital units and fleet capacity

Capital production is unlocked through research. Capital units can be researched and built early, rather than being restricted to a later doctrine or technology generation.

The player chooses one capital unit to start with. Production of capital units is also subject to fleet capacity.

After a capital design is unlocked, further research benefits capital units through passive stat improvements. Battle experience and rank provide their main individual progression and ability unlocks.

Fleet capacity can be increased through research. Supporting a larger army must also affect resource consumption. The exact model remains open, including whether the cost follows the researched capacity, actual army size, or both.

No upkeep percentage, fleet-capacity value, starting capital roster, or capital limit has been agreed.

## Pacing and strongest technologies

The standard match target remains approximately 90 minutes.

| Milestone | Intended timing |
| --- | --- |
| Early core unlocks, including basic tanks | Fast early progression. Exact times are open. |
| Doctrine choice | After completing the required core and meeting science-outpost requirements. Exact time is open. |
| Highest technology with a focused rush | Approximately 20 to 30 minutes into the match. |
| Highest technology during normal play | Approximately 30 to 40 minutes into the match. |

Reaching the highest technology does not by itself mean the match ends. The final reward and its battlefield effect remain undecided.

Possible final rewards include a superweapon building or a special capital unit. Neither is selected yet. Whether both doctrines share the reward or receive different rewards is also open.

## Questions needed to draw the tree

The remaining design questions are:

1. What units and buildings make up the core roster, and what is available before research?
2. What exclusive units, upgrades, and buildings define each doctrine?
3. How many technology generations are there, and what does each unlock?
4. Which technologies belong in the final core trees? The two Military and one Civilian completion requirement is agreed.
5. How should each doctrine affect later Civilian and Fleet research?
6. Which generation should contain each technology? Generation upgrades belong to outpost abilities.
7. How many science outposts does each stage require, and how do additional outposts affect research speed?
8. Which prerequisites depend on current outpost ownership after a generation has been reached?
9. Which capital units can players choose at the start, and what production research does that choice grant or require?
10. How does fleet-capacity research increase the army's resource consumption?
11. How many technologies and passive-upgrade levels belong in each part of the tree?
12. What is the strongest technology, and what counterplay does an opponent have?
13. What costs, durations, power requirements, queue rules, and interruption rules meet the pacing targets?

## Relationship to existing documents

The following documents provide context. Their older details are not automatically decisions in this plan:

- [Faction and research philosophy](faction-research.md) contains provisional faction names, identities, and doctrine examples. The infantry and tank split replaces those examples for the first human faction.
- [Gameplay rules](../game-rules.md) contains prototype costs, building prerequisites, fleet-capacity ideas, and separate Army and Civilian research centers. This plan uses one center with research categories.
- [Economy](economy.md) describes the competition between military production, expansion, and research. This plan specifies Materials and active Energy capacity as research costs.
- [Map and territory](map-territory.md) describes neutral outposts, capture, and specialization. This plan adds research-gated technology generations through science outposts.
- [Developer handbook](../developer-handbook.md) describes current building unlocks and the Supply Center name. Those implementation details remain separate from this planned progression.

The older documents have not been reconciled with this plan. Exact final tree nodes and balance values remain to be designed.

## Research overlay

The outline contains 44 provisional technologies across Military, Civilian, and Fleet chapter tabs. Each chapter contains connected trees with horizontal generation dividers. A progress label shows the Military and Civilian tree requirements for doctrines.

The Research button opens a large centered overlay over a dimmed battlefield. The match continues while battlefield commands and camera input are blocked. The close button or Escape dismisses the overlay.

Selecting a node shows its description, Material cost, duration, and prerequisites. Both doctrines remain inspectable. The screen omits annotations about how many match minutes it takes to reach a research tier.

The user requested real Material payments and refunds for this outline, followed by real timers and completion without gameplay unlocks. The implemented behavior is:

- Selecting a technology only inspects it. Clicking Start Research spends Materials and starts the timer immediately.
- Each chapter permits one active project. Different chapters run simultaneously.
- Projects use simulation ticks and continue while the overlay is closed.
- Cancelling an unfinished project refunds its full paid Material cost.
- Completion records the technology as researched, without unlocking units, buildings, abilities, or stat effects.
- Technologies require their earlier research links to be complete.
- A doctrine requires any two complete Military core trees and any one complete Civilian core tree.
- Completing one doctrine excludes the other. Cancelling an unfinished doctrine allows another choice.
- A prominent banner and an OR label present the doctrines as an exclusive choice. Each doctrine card names the path it excludes.
- Starting a doctrine dims the other doctrine and its exclusive technologies. Cancelling restores that path. Completion marks the other path as excluded.
- Power costs, gameplay effects, and outpost generation upgrades remain previews.

A completed Research Facility hosts research projects. Construction units can build one for 1,200 Materials; the facility has 600 health and consumes 15 energy, equal to the output of 1.5 Power Plants. Active projects consume additional energy. Without an owned, living facility, the research window remains accessible and shows a warning, but new research cannot start. Losing a project's facility pauses that project. Cancellation remains available for a full refund.

The outline's nodes, costs, durations, and tier assignments are placeholders. They do not settle the remaining questions about the final research tree.
