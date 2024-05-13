import configparser
import requests
from db_sql_connector import SqlDatabaseConnector
import torch
from bert_score import score
from transformers import AutoTokenizer, AutoModelForCausalLM, AutoModelForMaskedLM

class GemmaModelEvaluator:
    def __init__(self):
        self.api_config = self.load_api_configuration()
        
        self.connector = SqlDatabaseConnector()

        self.df_instructions = self.connector.fetch_instructions()        
        self.tokenizer = AutoTokenizer.from_pretrained("google/gemma-2b-it")
        self.model = AutoModelForCausalLM.from_pretrained("google/gemma-2b-it", torch_dtype=torch.bfloat16)        

    def load_api_configuration(self):
        config = configparser.ConfigParser()
        config.read('config.ini')
        return config['api']
    
    def build_prompt(self, instruction):
        prompt = """
            <start_of_turn>user
            Answer the following question in a concise and informative manner. The question is in spanish, then answer in spanish.
            {instruction}<end_of_turn>
            <start_of_turn>model
        """
        prompt = prompt.format(instruction=instruction)
        return prompt

    def augment_query(self, instruction):
        url = self.api_config['query_augmentation_url']
        
        data = {
            "model": "gemma",
            "query": instruction,
            "userId": "69A03A54-4181-4D50-8274-D2D88EA911E4"
        }

        response = requests.post(url, json=data, verify=False)
        response.raise_for_status()

        return response.json()['query']
    
    def extract_model_response(self, response):
        response = response.replace('<bos>', '')
        response = response.replace('<eos>', '')
        response = response.split('<start_of_turn>model')[1]
        response = response.strip()
        return response

    def evaluate(self):
        for index, row in self.df_instructions.iterrows():
            instruction = row['Instruction']            
            instruction_prompt = self.build_prompt(instruction)
            instruction_tokens = self.tokenizer(instruction_prompt, return_tensors="pt")

            gemma_completion = self.model.generate(**instruction_tokens, max_new_tokens=1000)
            
            response = self.tokenizer.decode(gemma_completion[0])
            model_reponse = self.extract_model_response(response)
            
            instruction_id = row['Id'] 

            query = f"INSERT INTO [datasets].[BertScoreEvaluationMetrics](InstructionId, LargeLanguageModel, Language, LlmResponseCompletion) "
            query = query + f" VALUES({instruction_id}, 'gemma', 'es-MX', '{model_reponse}')"

            self.connector.save_response(query)

    def evaluate_with_rag(self):
        for index, row in self.df_instructions.iterrows():
            instruction = row['Instruction']
            augmented_instruction = self.augment_query(instruction)
            
            instruction_tokens = self.tokenizer(augmented_instruction, return_tensors="pt")
            gemma_completion = self.model.generate(**instruction_tokens, max_new_tokens=1000)
            
            response = self.tokenizer.decode(gemma_completion[0])
            model_reponse = self.extract_model_response(response)
            
            instruction_id = row['Id'] 

            query = f"INSERT INTO [datasets].[RagBertScoreEvaluationMetrics](InstructionId, LargeLanguageModel, Language, LlmResponseCompletion) "
            query = query + f" VALUES({instruction_id}, 'gemma', 'es-MX', '{model_reponse}')"

            self.connector.save_response(query)

    def measure_performance(self):
        
        #eval_model = AutoModelForMaskedLM.from_pretrained("google-bert/bert-base-multilingual-cased")

        df_llm_responses = self.connector.fetch_llm_responses()

        llm_response = df_llm_responses['LlmCompletion'].tolist()
        correct_response = df_llm_responses['Label'].tolist()

        P, R, F1 = score(llm_response, correct_response, lang="es", verbose=False)

        print(P.numpy())
        print(R.numpy())
        print(F1.numpy())
