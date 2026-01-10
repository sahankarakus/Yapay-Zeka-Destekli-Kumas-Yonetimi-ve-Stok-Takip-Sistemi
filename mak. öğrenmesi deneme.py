import pandas as pd
import numpy as np
from sklearn.linear_model import LinearRegression
from sklearn.preprocessing import LabelEncoder
from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import accuracy_score
from sklearn.tree import DecisionTreeClassifier, export_graphviz 
import graphviz
import xgboost as xgb
from sklearn.ensemble import RandomForestRegressor
import matplotlib.pyplot as plt
from sklearn.neighbors import KNeighborsClassifier
from sklearn.svm import SVC
from sklearn.naive_bayes import GaussianNB,CategoricalNB
from sklearn.model_selection import GridSearchCV


### VERİ SETİ EKLE ###
pd.set_option('display.max_columns', None) #tüm sütunları gösterir
pd.set_option('display.width', 100)
path= r"C:\Users\sahan\OneDrive\Desktop\Dersler\4. sınıf\Mühendislik\sona doğru\kumas_verisi_1000.xlsx"
veri= pd.read_excel(path)


### Kullanım alanı sütununu enceode et ###
le=LabelEncoder()
le.fit(veri['Kullanim_Alani'])
y_encoded=le.transform(veri['Kullanim_Alani'])
veri['Kullanim_alani_encoded']=y_encoded


### Veri setinden eğitim için gereksiz sütunlar çıkar ###
veri=veri.drop(columns=['ID','Kumas_Ad','Kumas_Renk','Kumas_Uzunluk_m','Kumas_En_cm'])
#print(veri.info())# sütunların veri türlerini söyler


veri=pd.get_dummies(veri,columns=['Kumas_Tur','Kumas_Likra_Yonu','Kumas_SuItici','Kullanim_Donemi'],drop_first=True)
#print(veri.head(3))


### hedef sütun ve future belirleme ###
y= veri[['Kullanim_alani_encoded']]
x=veri.drop(['Kullanim_alani_encoded','Kullanim_Alani'],axis=1)

### TRAİN TEST AYIRMA ###
x_train,x_test,y_train,y_test=train_test_split(x,y,train_size=0.80,random_state=7)


"""
### RANDOM FOREST CLASSİFİER ###

forest =RandomForestClassifier(n_estimators=350, max_depth=10,min_samples_split=4,random_state=5)
print("Model eğitiliyor...", flush=True)
model=forest.fit(x_train,y_train)
score = model.score(x_test, y_test)
print(f"Random Forest öğrenme skoru: {score}", flush=True)
"""

### XGBoosting ###
"""
rf=xgb.XGBClassifier(max_depth=15,learning_rate=0.08)
model=rf.fit(x_train,y_train)
print("xgboost skoru ",model.score(x_test,y_test))
    """ 
### KARAR AĞACI (DESİCİON TREE) ALGORİTMASI###
"""
tree= DecisionTreeClassifier(max_depth=12,min_samples_split=20)
model=tree.fit(x_train,y_train)
print("karar ağacı Öğrenme skoru:",model.score(x_test,y_test),"\n")
"""

### BAYES ###
"""
bys= CategoricalNB()
model=bys.fit(x_train, y_train)
print("bayes", model.score(x_test, y_test))
"""


### KNN ###
"""
k_nn=KNeighborsClassifier(n_neighbors=5)
model=k_nn.fit(x_train,y_train)
print("KNN ",model.score(x_test,y_test))

"""
### SVM ###
"""
sVm=SVC(kernel='rbf',C=1.0)
model=sVm.fit(x_train,y_train)
print("SVM" ,model.score(x_test,y_test))
"""


### LİNEER REGRESSİON MODELİ ###

lm=LinearRegression()
model=lm.fit(x_train,y_train)
print("Lineer regresyon",model.score(x_test,y_test))



### DENEME KODLARI###
"""
denemex=np.array(x.iloc[689])
print("deneme tahmini",model.predict([denemex]),"\n" )
print("doğrusu",y.iloc[689])
"""