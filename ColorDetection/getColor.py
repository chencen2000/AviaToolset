import xlrd
from sklearn.neighbors import KNeighborsClassifier
from sklearn import metrics
import argparse


def addTrainData(sheet, C, label, x, y):
    for i in C:
        colorData = sheet.row_values(i)
        x.append(colorData[4:10])
        y.append(label)
    return x, y



# construct the argument parser and parse the arguments
ap = argparse.ArgumentParser()
ap.add_argument("-x1", "-x1", type=float, required=True,
	help="input x1")
ap.add_argument("-x2", "-x2", type=float, required=True,
	help="input x2")
ap.add_argument("-x3", "-x3", type=float, required=True,
	help="input x3")
ap.add_argument("-x4", "-x4", type=float, required=True,
	help="input x4")
ap.add_argument("-x5", "-x5", type=float, required=True,
	help="input x5")
ap.add_argument("-x6", "-x6", type=float, required=True,
	help="input x6")
args = vars(ap.parse_args())

loc = ("Color_Sensor_Test_Report.xlsx")

wb = xlrd.open_workbook(loc)
sheet = wb.sheet_by_index(0)

sheet.cell_value(0, 0)

# Data for one iphone model
C1 = [15, 16, 17, 18, 19, 20, 88, 89, 90, 91, 92, 93]    #iphone7 gold
C2 = [30, 31, 32, 33, 34, 35, 36, 37]  #iphone7 black
C3 = [112, 113, 114]     #iphone7 red

x = []
y = []

# generate Training data for the phone model
x, y = addTrainData(sheet, C1, 0, x, y)
x, y = addTrainData(sheet, C2, 1, x, y)
x, y = addTrainData(sheet, C3, 2, x, y)

k = 3             #the k parameter for KNN
knn = KNeighborsClassifier(n_neighbors=k)
knn.fit(x, y)
x_test = [[args["x1"], args["x2"], args["x3"], args["x4"], args["x5"], args["x6"]]]
y_label = ['Gold', 'Black', 'Red']
y_pred = knn.predict(x_test)

print(y_label[y_pred[0]])


