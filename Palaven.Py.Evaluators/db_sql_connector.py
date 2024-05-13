import pandas as pd
import pyodbc
import configparser

class SqlDatabaseConnector:
    def __init__(self):
        config = self.load_configuration()

        username = config['username']
        password = config['password']
        server = config['server']
        database = config['database']
        
        self.connectionString = f'DRIVER={{ODBC Driver 18 for SQL Server}};SERVER={server};DATABASE={database};UID={username};PWD={password}'        

    def load_configuration(self):
        config = configparser.ConfigParser()
        config.read('config.ini')
        return config['database']

    def fetch_instructions(self):
        conn = pyodbc.connect(self.connectionString)
        query = 'SELECT [Id],[Instruction],[Response] FROM [palaven].[datasets].[Instructions] WHERE [Id] BETWEEN 1 AND 50'
        df = pd.read_sql_query(query, conn)
        conn.close()
        return df
    
    def fetch_llm_responses(self):
        conn = pyodbc.connect(self.connectionString)
        query = 'SELECT LargeLanguageModel, EvaluationId, ExcerciseDescription, [Label],[LlmCompletion] FROM datasets.vwLlmResponses'
        df = pd.read_sql_query(query, conn)
        conn.close()
        return df
    
    def fetch_llm_with_rag_responses(self):
        conn = pyodbc.connect(self.connectionString)
        query = 'SELECT LargeLanguageModel, EvaluationId, ExcerciseDescription, [Label],[LlmCompletion] FROM datasets.vwLlmWithRagResponses'
        df = pd.read_sql_query(query, conn)
        conn.close()
        return df
    
    def save_response(self, query):
        conn = pyodbc.connect(self.connectionString)
        cursor = conn.cursor()
        cursor.execute(query)
        conn.commit()
        cursor.close()
        conn.close()        
