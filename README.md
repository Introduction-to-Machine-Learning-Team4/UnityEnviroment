## CrossyRoad 
EXE is under Execuatable folder

## Document
More Document about MLagent is at https://github.com/Unity-Technologies/ml-agents/blob/release_19_docs/docs/Readme.md <br />
Useful Doc:
- [API Docs/Python API Documentation](https://github.com/Unity-Technologies/ml-agents/blob/release_19_docs/docs/Python-API-Documentation.md)
- [API Docs/How to use the Python API](https://github.com/Unity-Technologies/ml-agents/blob/release_19_docs/docs/Python-API.md)
- [Python Tutorial with Google Colab/Using a UnityEnvironment](https://colab.research.google.com/github/Unity-Technologies/ml-agents/blob/release_19_docs/colab/Colab_UnityEnvironment_1_Run.ipynb)
- [Python Tutorial with Google Colab/Q-Learning with a UnityEnvironment](https://colab.research.google.com/github/Unity-Technologies/ml-agents/blob/release_19_docs/colab/Colab_UnityEnvironment_2_Train.ipynb) 

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
Continuous Action: 0 <br />
Discrete Action: 1 <br />
- Branch size: 5 <br />
0: No Movement/1: Front/2: Back/3: Left/4: Right

## Observation Space
Total size: 60 <br />
30 feature obsered and with 2 stacked vector.
- size 2: Player Coordinate(X,Z)
- size 4: The type of line which relative to player(previous,current,next two)<br />
type 0: Grass, 1: Road, 2: Water
- size 2: The Obstacles Coordinate(X,Z)<br />
3 obstacle observed per line. 6 feature per line. Total 24 feature.

## Changelogs
- v2.1: Reward should add correctly when beating the high score
- v2.0: Observation size now change to 60.<br/>
add Player coordinate, Line type, Obstacle coordinate to observation
- v1.0: Executable Create. Observation space size = 3
