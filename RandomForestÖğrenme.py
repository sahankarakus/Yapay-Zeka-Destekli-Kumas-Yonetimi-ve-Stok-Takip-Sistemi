import pandas as pd
from sklearn.preprocessing import LabelEncoder
from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestClassifier
import joblib
import sklearn
import sys

print(f"sklearn version: {sklearn.__version__}", flush=True)
print("Model eğitimi başlıyor...", flush=True)

### VERİ SETİ EKLE ###
pd.set_option('display.max_columns', None) #tüm sütunları gösterir
pd.set_option('display.width', 100)
path= r"C:\Users\sahan\OneDrive\Desktop\kumas_verisi_1000.xlsx"
veri= pd.read_excel(path)

# VERİ SETİNDEKİ SÜTUNLARIN VERİ TÜRLERİ#
#print(veri.dtypes)

### Kullanım alanı sütununu enceode et ###
le=LabelEncoder()
le.fit(veri['Kullanim_Alani'])
y_encoded=le.transform(veri['Kullanim_Alani'])
veri['Kullanim_alani_encoded']=y_encoded


### Veri setinden eğitim için gereksiz sütunlar çıkar ###
veri=veri.drop(columns=['ID','Kumas_Ad','Kumas_Renk','Kumas_Uzunluk_m','Kumas_En_cm'])
#print(veri.info())# sütunların veri türlerini söyler

#Kategorik verileri sayısala çevir#
veri=pd.get_dummies(veri,columns=['Kumas_Tur','Kumas_Likra_Yonu','Kumas_SuItici','Kullanim_Donemi'],drop_first=False)
#print(veri.head(3))


### hedef sütun ve future belirleme ###
y= veri[['Kullanim_alani_encoded']]
x=veri.drop(['Kullanim_alani_encoded','Kullanim_Alani'],axis=1)

### TRAİN TEST AYIRMA ###
x_train,x_test,y_train,y_test=train_test_split(x,y,train_size=0.80,random_state=7)



### RANDOM FOREST CLASSİFİER ###

forest =RandomForestClassifier(n_estimators=350, max_depth=10,min_samples_split=4,random_state=5)
print("Model eğitiliyor...", flush=True)
model=forest.fit(x_train,y_train)
score = model.score(x_test, y_test)
print(f"Random Forest öğrenme skoru: {score}", flush=True)




# 1. Eğitilmiş Modeli Kaydet
print("Model kaydediliyor...", flush=True)
model_path = r"C:\Users\sahan\OneDrive\Desktop\Dersler\4. sınıf\Mühendislik\sona doğru\kumas_model_rf.pkl"
joblib.dump(model, model_path)
print(f"Model kaydedildi: {model_path}", flush=True)

# 2. Label Encoder'ı Kaydet (Sonuçta çıkan 0,1,2'yi tekrar 'Gömlek','Pantolon' yapabilmek için)
encoder_path = r"C:\Users\sahan\OneDrive\Desktop\Dersler\4. sınıf\Mühendislik\sona doğru\label_encoder.pkl"
joblib.dump(le, encoder_path)
print(f"Label encoder kaydedildi: {encoder_path}", flush=True)

# 3. Sütun İsimlerini Kaydet (KRİTİK NOKTA: get_dummies sonrası oluşan sütun yapısını saklıyoruz)
columns_path = r"C:\Users\sahan\OneDrive\Desktop\Dersler\4. sınıf\Mühendislik\sona doğru\model_columns.pkl"
joblib.dump(x.columns, columns_path)
print(f"Model sütunları kaydedildi: {columns_path}", flush=True)

print("Tüm dosyalar başarıyla kaydedildi!", flush=True)
