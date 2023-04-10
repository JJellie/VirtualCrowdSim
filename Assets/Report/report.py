# data visualisation
import matplotlib.pyplot as plt 

# data wrangling
import pandas as pd
import numpy as np

# data loading
data = pd.read_csv('./Assets/Report/collisions.csv')

data.columns = ["start_frame", "end_frame", "relativeV_x", "relativeV_y", "relativeV_z"];

print(data.info())

for i in [2,3,4,100,6,7]:
    print(data['relativeV_x'][0:200])