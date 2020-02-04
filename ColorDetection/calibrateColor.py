import pandas as pd
from sklearn.neighbors import KNeighborsClassifier

def load_data(fn):
    data = pd.read_csv(fn)
    return data


def gen_color_label(df):
    color=df["Color"]
    label=pd.Series(range(0,100))
    data=pd.DataFrame({"label": label, "color": color})
    return data


def prepare_traindata(data):
    x = []
    y = []
    for index, row in data.iterrows():
        x.append([int(row["R"]),int(row["G"]),int(row["B"]),int(row["C"]),int(row["Lux"]),int(row["Color Temp"])])
        y.append(index)
    return x,y


if __name__ == "__main__":
    # print("hello, world")

    data0=load_data('ColorSensor_001.csv')
    data=load_data('ColorSensor_006.csv')
    x, y =prepare_traindata(data)
    k=1
    knn = KNeighborsClassifier(n_neighbors=3)
    knn.fit(x, y)
    x_test = [[1317,1305,1388,3841,616,11775]]
    y_pred = knn.predict(x_test)
    print(y_pred)
    d1 = data.loc[y_pred[0]]
    print(d1)
    color_name = data.loc[y_pred[0]]["Color"]
    d0=data0.loc[data0["Color"]==color_name]
    print(d0)
    dr= (float(d1["R"]) -float(d0["R"]))/float(d0["R"])
    dg= (float(d1["G"]) -float(d0["G"]))/float(d0["G"])
    db= (float(d1["B"]) -float(d0["B"]))/float(d0["B"])
    dc= (float(d1["C"]) -float(d0["C"]))/float(d0["C"])
    rr = float(x_test[0]) / (dr+1)
    rg = float(x_test[1]) / (dg+1)
    rb = float(x_test[2]) / (db+1)
    rc = float(x_test[3]) / (dc+1)
    
