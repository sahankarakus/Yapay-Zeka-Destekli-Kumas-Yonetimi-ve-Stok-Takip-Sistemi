
import pandas as pd
import sqlite3
import joblib
import sys
import os
import numpy as np

# Import helper functions from tahmin_yap.py
# Assuming tahmin_yap.py is in the same directory
try:
    from tahmin_yap import load_models, preprocess_data, DB_PATH
except ImportError:
    # If import fails, we might be running from a different cwd, try to adjust or fail
    print("Error: Could not import from tahmin_yap.py. Make sure you are in the correct directory.")
    sys.exit(1)

INPUT_EXCEL = "veritabanıDoldurma.xlsx"

def import_and_predict():
    print(f"Loading data from {INPUT_EXCEL}...")
    try:
        df_excel = pd.read_excel(INPUT_EXCEL)
    except Exception as e:
        print(f"Error loading Excel file: {e}")
        return

    print("Loading models...")
    model, encoder, training_columns, error_msg = load_models()
    if model is None:
        print(f"Error loading models: {error_msg}")
        return

    print("Preprocessing data for prediction...")
    # Preprocess expects specific columns. We pass the whole DF, it filters what it needs.
    # Note: Kumas_Ad, etc. are dropped in preprocess_data, so we need to keep the original DF for DB insertion.
    
    processed_data, error_msg = preprocess_data(df_excel, training_columns)
    if processed_data is None:
        print(f"Error preprocessing data: {error_msg}")
        return

    # Ensure all features are numeric for the model (convert datetimes/objects to numeric where possible)
    try:
        processed_data = processed_data.apply(pd.to_numeric, errors='coerce').fillna(0)
    except Exception as e:
        print(f"Error converting processed features to numeric: {e}")
        return

    print("Running predictions...")
    try:
        # Batch prediction
        # predict_proba returns (n_samples, n_classes)
        predictions_proba = model.predict_proba(processed_data)
        
        # Get the index of the max probability for each row
        prediction_indices = predictions_proba.argmax(axis=1)
        
        # Decode the indices to class names
        predicted_labels = encoder.inverse_transform(prediction_indices)
        
        # Update the DataFrame
        df_excel['Kullanim_Alani'] = predicted_labels
        
        print("Predictions completed.")
        print(df_excel[['Kumas_Ad', 'Kullanim_Alani']].head())
        
    except Exception as e:
        print(f"Error during prediction: {e}")
        return

    print("Saving to database...")
    try:
        conn = sqlite3.connect(DB_PATH)
        
        # We need to match the DB schema.
        # Check if 'ID' column exists in Excel and if we should use it.
        # If ID exists in Excel, we can try to use it. If it conflicts, we might need to drop it.
        # Let's try to append. If ID is present, it will be used.
        
        # 'ID' is usually Primary Key. If Excel has IDs that match existing DB IDs, it will crash.
        # If Excel IDs are just row numbers or we want new IDs, we should drop 'ID' column if it exists in DF
        # allowing DB to auto-increment.
        
        # Check if DB has data
        cursor = conn.cursor()
        cursor.execute("SELECT max(ID) FROM KullaniciniKumaslari")
        max_id = cursor.fetchone()[0]
        
        if max_id is None:
            max_id = 0
            
        print(f"Current Max ID in DB: {max_id}")
        
        # If the Excel file 'ID' column is just 1, 2, 3... it might conflict if not careful.
        # Safe bet: Drop ID from dataframe and let SQLite autoincrement it?
        # OR: specific user request "veritabanıDoldurma dosyasını...". 
        # Usually importers treat the file as new data.
        
        if 'ID' in df_excel.columns:
            # Let's verify if these IDs overlap or if we should ignore them.
            # For this task, I'll drop ID and let the DB generate new unique IDs to avoid conflicts,
            # unless the user specifically wants to UPDATE existing rows (not specified).
            print("Dropping 'ID' column from Excel data to allow Auto-Increment in DB.")
            df_excel_to_save = df_excel.drop(columns=['ID'])
        else:
            df_excel_to_save = df_excel
            
        # Append to table
        df_excel_to_save.to_sql('KullaniciniKumaslari', conn, if_exists='append', index=False)
        
        print(f"Successfully saved {len(df_excel)} rows to 'KullaniciniKumaslari' table.")
        conn.close()
        
    except Exception as e:
        print(f"Error saving to database: {e}")

if __name__ == "__main__":
    import_and_predict()
