import time
import requests

class PalavenApi:
    def __init__(self, palaven_base_url):
        self.palaven_base_url = palaven_base_url
    
    def _get_request(self, url, params=None, headers=None):
        """
        Performs a GET request to the specified URL.

        :param url: URL of the API to make the request to.
        :param params: Query parameters to send in the GET request.
        :param headers: Headers to include in the request.
        :return: JSON response from the API.
        """
        try:
            url = f'{self.palaven_base_url}{url}'
            response = requests.get(url, params=params, headers=headers)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.HTTPError as http_err:
            print(f'HTTP error occurred: {http_err}')
        except Exception as err:
            print(f'Other error occurred: {err}')


    def _post_request(self, url, json=None, files=None, headers=None):
        """
        Performs a POST request to the specified URL with the provided data.

        :param url: URL of the API to make the request to.
        :param json: JSON data to send in the POST request.
        :param files: Files to send in the POST request.
        :param headers: Headers to include in the request.
        :return: JSON response from the API.
        """
        try:
            url = f'{self.palaven_base_url}{url}'
            response = None
            if files:
                data = {key: (None, str(value)) for key, value in files.items()}
                response = requests.post(url, files=data, headers=headers)
            else:
                response = requests.post(url, json=json, headers=headers)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.HTTPError as http_err:
            print(f'HTTP error occurred: {http_err}')
        except Exception as err:
            print(f'Other error occurred: {err}')


    def fetch_active_evaluation_session(self, dataset_id):
        """
        Fetches the active evaluation session for a given dataset.

        :param dataset_id: ID of the dataset.
        :return: JSON response from the API.
        """
        url = f"/evaluationsession/active/dataset/{dataset_id}"        
        response = self._get_request(url)
        return response


    def create_evaluation_session(self, dataset_id, batch_size, llm, device_info):
        """
        Creates a new evaluation session.

        :param dataset_id: ID of the dataset.
        :param batch_size: Size of the batch.
        :param llm: Large language model to use.
        :param device_info: Information about the device.
        :return: JSON response from the API.
        """
        url = "/evaluationsession"
        body = {
            'datasetId': f'{dataset_id}',
            'batchSize': batch_size,
            'largeLanguageModel': f'{llm}',
            'deviceInfo': f'{device_info}'
        }

        response = self._post_request(url, json=body)
        return response

    def fetch_instruction_test_dataset(self, evaluation_session_id, batch_number, evaluation_exercise=None):
        """
        Fetches the instruction test dataset for a given evaluation session.

        :param evaluation_session_id: ID of the evaluation session.
        :param batch_number: Number of the batch.
        :param evaluation_exercise: Optional evaluation exercise name.
        :return: JSON response from the API.
        """
        url = f"/evaluationsession/{evaluation_session_id}/dataset/instructions?batchNumber={batch_number}" if evaluation_exercise is None else \
            f"/evaluationsession/{evaluation_session_id}/chatcompletion/{evaluation_exercise}/instructions?batchNumber={batch_number}"
        
        response = self._get_request(url)
        return response

    def generate_prompt(self, llm, instruction):
        """
        Generates a formatted prompt for a given instruction and a given large language model.

        :param llm: Large language model to use.
        :param instruction: Instruction to generate the prompt for.
        :return: JSON response from the API.
        """
        url = f'/chatcompletion/{llm}/prompt'
        form = {'query': instruction}
                            
        response = self._post_request(url, files=form)
        return response

    def generate_augmented_prompt(self, llm, instruction, topK=1, minMatchScore=0.8):
        """
        Generates an augmented formatted prompt for a given instruction and a given large language model.

        :param llm: Large language model to use.
        :param instruction: Instruction to generate the prompt for.
        :param topK: Number of top matches to return.
        :param minMatchScore: Minimum match score.
        :return: JSON response from the API.
        """
        url = f'/chatcompletion/{llm}/prompt/augment'
        form = {
            'query': instruction,
            'minMatchScore': minMatchScore,
            'topK': topK
        };    
                            
        response = self._post_request(url, files=form)
        return response

    def save_model_response(self, evaluation_session_id, evaluation_exercise, batch_number, instruction_id, chat_completion, elapsed_time):
        """
        Saves the model response for a given evaluation session and a given exercise.

        :param evaluation_session_id: ID of the evaluation session.
        :param evaluation_exercise: Evaluation exercise name.
        :param batch_number: Number of the batch.
        :param instruction_id: ID of the instruction.
        :param chat_completion: Chat completion response.
        :param elapsed_time: Elapsed time for the response.
        :return: JSON response from the API.
        """
        start_time = time.time()

        url = f"/evaluationsession/{evaluation_session_id}/chatcompletion/{evaluation_exercise}/response"
        form = {
            'batchNumber': batch_number,
            'instructionId': instruction_id,
            'responseCompletion': chat_completion,
            'elapsedTime': elapsed_time
        }

        response = self._post_request(url, files=form)

        end_time = time.time()    
        elapsed_time = end_time - start_time
        print(f'Palaven.SaveResponse. InstructionId: {instruction_id},  Elapsed-Time: {elapsed_time:.2f} seconds')

        return response

    def fetch_model_responses(self, evaluation_session_id, evaluation_exercise, batch_number):
        """
        Fetches the model responses for a given evaluation session and a given exercise.

        :param evaluation_session_id: ID of the evaluation session.
        :param evaluation_exercise: Evaluation exercise name
        :param batch_number: Number of the batch.
        :return: JSON response from the API.
        """
        url = f"/evaluationsession/{evaluation_session_id}/chatcompletion/{evaluation_exercise}/responses?batchNumber={batch_number}"
        response = self._get_request(url)
        return response

    def save_bert_score_metrics(self, evaluation_session_id, evaluation_exercise, batch_number, precision, recall, f1):
        """
        Saves the BERT score metrics for a given evaluation session and a given exercise.

        :param evaluation_session_id: ID of the evaluation session.
        :param evaluation_exercise: Evaluation exercise name.
        :param batch_number: Number of the batch.
        :param precision: Accuracy score.
        :param recall: Recall score.
        :param f1: F1 score.
        :return: JSON response from the API.
        """
        url = f"/evaluationsession/{evaluation_session_id}/metrics/{evaluation_exercise}/bertscore"
        body = {
            'batchNumber': batch_number,
            'precision': precision,
            'recall': recall,
            'f1': f1
        }

        response = self._post_request(url, json=body)
        return response

    def fetch_bert_score_metrics(self, evaluation_session_id, evaluation_exercise):
        """
        Fetches the BERT score metrics for a given evaluation session and a given exercise.

        :param evaluation_session_id: ID of the evaluation session.
        :param evaluation_exercise: Evaluation exercise name.
        :return: JSON response from the API.
        """
        url = f"/evaluationsession/{evaluation_session_id}/metrics/{evaluation_exercise}/bertscore"
        response = self._get_request(url)
        return response

    def save_rouge_metrics(self, evaluation_session_id, evaluation_exercise, batch_number, rouge_scores):
        """
        Saves the ROUGE metrics for a given evaluation session and a given exercise.

        :param evaluation_session_id: ID of the evaluation session.
        :param evaluation_exercise: Evaluation exercise name.
        :param batch_number: Number of the batch.
        :param rouge_scores: ROUGE scores to save
        :return: JSON response from the API.
        """
        url = f"/evaluationsession/{evaluation_session_id}/metrics/{evaluation_exercise}/rougescore"
        response = self._post_request(url, json=rouge_scores)
        return response

    def fetch_rouge_metrics(self, evaluation_session_id, evaluation_exercise, rouge_type):
        """
        Fetches the ROUGE metrics for a given evaluation session and a given exercise.

        :param evaluation_session_id: ID of the evaluation session.
        :param evaluation_exercise: evaluation exercise name.
        :param rouge_type: Type of ROUGE metric to fetch.
        :return: JSON response from the API.
        """
        url = f"/evaluationsession/{evaluation_session_id}/metrics/{evaluation_exercise}/rougescore?rougeType={rouge_type}"
        response = self._get_request(url)
        return response

    def save_bleu_score_metrics(self, evaluation_session_id, evaluation_exercise, batch_number, score):
        """
        Saves the BLEU score metrics for a given evaluation session and a given exercise.

        :param evaluation_session_id: ID of the evaluation session.
        :param evaluation_exercise: Evaluation exercise name.
        :param batch_number: Number of the batch.
        :param score: BLEU score.
        :return: JSON response from the API.
        """
        url = f"/evaluationsession/{evaluation_session_id}/metrics/{evaluation_exercise}/bleuscore"
        body = {
            'batchNumber': batch_number,
            'bleuScore': score,
        }

        response = self._post_request(url, json=body)
        return response

    def fetch_bleu_score_metrics(self, evaluation_session_id, evaluation_exercise):
        """
        Fetches the BLEU score metrics for a given evaluation session and a given exercise.

        :param evaluation_session_id: ID of the evaluation session.
        :param evaluation_exercise: Evaluation exercise name.
        :return: JSON response from the API.
        """
        url = f"/evaluationsession/{evaluation_session_id}/metrics/{evaluation_exercise}/bleuscore"
        response = self._get_request(url)
        return response