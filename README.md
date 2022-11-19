## CrossyRoad 

EXE is under Execuatable folder

## Installation

1. create an enviroment with **Python 3.6 or 3.7**
2. Install the pytorch from https://pytorch.org/get-started/locally/
3. Install the mlagent with pip
```
python -m pip install mlagents==0.28.0
```
4. Install importlib-metadata
```
pip install importlib-metadata==4.4
```
More Installation Detail at https://github.com/Unity-Technologies/ml-agents/blob/release_19_docs/docs/Installation.md

## Usage (Command Line)
Run the MLAgent Default Model(PPO/SAC) by Anaconda command prompt under the folder with exe
```
mlagents-learn <config path> --env=<exe name> --run-id=<run_name>
```
It should be like
```
mlagents-learn config\player_config.yaml --env="CRML" --run-id=test
```
## Usage (Python)
To load a Unity environment from a built binary file, put the file in the same directory
as enviroment(exe), run:
```python
from mlagents_envs.environment import UnityEnvironment
# This is a non-blocking call that only loads the environment.
env = UnityEnvironment(file_name="CRML", seed=1, side_channels=[])
# Start interacting with the environment.
env.reset()
behavior_names = env.behavior_specs.keys()
...
```
more Details at https://github.com/Unity-Technologies/ml-agents/blob/release_19_docs/docs/Python-API.md

## Action Space

0: No Movement
1: Front
2: Back
3: Left
4: Right

## Observation Space

size 3: Player Coordinate
