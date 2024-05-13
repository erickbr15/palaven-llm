from gemma_evaluator import GemmaModelEvaluator

def main():    
    evaluator = GemmaModelEvaluator()
    evaluator.evaluate()
    evaluator.evaluate_with_rag()
    evaluator.measure_performance()

if __name__ == "__main__":
    main()