# **Semantic Analysis Of Textual Data**

## **Project Overview**

This project presents a structured approach to **semantic similarity analysis of textual data**, addressing the need for **effective processing and extraction of insights from unstructured content**. The framework is designed with a **modular architecture**, ensuring adaptability across different domains while maintaining **computational efficiency** through **parallel processing and chunking techniques**.

The system delivers **accurate and meaningful results** while optimizing performance, making it a scalable and efficient solution for various applications, including **document clustering, content recommendation, anomaly detection, and information retrieval**.

---

## **Project Workflow**

### **Flow Diagram**

The following diagram represents the complete project workflow.
  ![Screenshot 2025-03-29 212133](https://github.com/user-attachments/assets/102154c6-b450-4dfc-8d37-3c366956ff0a)


### 1. **Document Cleaning and Extraction**

- **Input**: Two Raw documents (e.g., .docx, PDF, .md, csv).
- **Process**:
  - Text cleaning: Remove noise, special characters, and unnecessary formatting.
  - Filtering: Extract relevant information for analysis.
- **Output**: Two structured JSON files representing the cleaned and filtered data.

---

### 2. **Embedding Generation**

- **Input**: JSON files created in the previous step.
- **Process**:
  - Utilizing the JSON files.
  - Generate embeddings for the text using OpenAI's embedding models.
- **Output**: CSV files with two columns:
  - **Text**: The original extracted data.
  - **Embedding**: Numerical vectors representing semantic meaning.

---

### 3. **Cosine Similarity Calculation**

- **Input**: CSV files with embeddings.
- **Process**:
  - Calculate the cosine similarity between embeddings from two CSV input files to measure the semantic similarity between text data points.
- **Output**: A CSV file representing a similarity matrix.

---

## **Project Structure**

```
📂 Semantic_Analysis
│-- 📂 CSVOutput/              # Folder storing CSV files
│-- 📂 EmbeddingOutput/        # Folder storing generated embeddings
│-- 📂 ExtractedData/          # Folder containing input documents
│-- 📂 Interfaces/             # Folder containing interface definitions
│-- 📂 RawData/                # Folder for unprocessed files
│-- 📜 appsettings.json        # Configuration file
│-- 📜 CosineSimilarity.cs     # Script for cosine similarity calculation
│-- 📜 DataExtraction.cs       # Script for document processing
│-- 📜 EmbeddingProcessor.cs   # Script for generating embeddings
│-- 📜 Program.cs              # Main entry point

📂 UnitTestProject
│-- 📜 CosineSimilarityUnitTest.cs  # Unit tests for cosine similarity
│-- 📜 DataExtractionTest.cs        # Unit tests for data extraction
│-- 📜 EmbeddingUnitTest.cs         # Unit tests for embedding processor
```

---
## **Setup Instructions for Running the Project**

### **Prerequisites**

- .NET SDK
- Ensure that the API key is set as an environment variable with the name OPENAI_API_KEY.
  
### Steps to Run the Program  

1. **Clone the repository.**
```sh
git clone https://github.com/PatelAman09/semantic-analysis.git
```    
2. **Build the project** located at:  "semantic-analysis\Semantic_Analysis\Semantic_Analysis"  
```sh
dotnet build
```
3. Place your raw documents (the two files you want to compare) in: "semantic-analysis\Semantic_Analysis\Semantic_Analysis\RawData"
4. Ensure the following folders are empty before running the program:
- ExtractedData
- Embeddingoutput
- CSVOutput
5. Navigate to bin/Debug/(Your .NET version) and execute **Semantic_Analysis.exe**
6. After running the console application do the data extraction from raw files and in embedding generation step user will get following prompt for both files:
  ![Screenshot 2025-03-29 234713](https://github.com/user-attachments/assets/73950ba5-c069-48d2-8090-842557e40468)
  For both files, the user can choose the embedding type: (1)word-by-word, phrase-by-phrase, or (2)entire document.
7. The final output file, containing the cosine similarity calculation, is located at: "semantic-analysis\Semantic_Analysis\Semantic_Analysis\CSVOutput"
---

## Sample Result from **CSVOutput** folder: Screenshot of Cosine Similarity Calculation  
  ![Screenshot 2025-03-30 000306](https://github.com/user-attachments/assets/f402eba7-ffa6-437a-9f14-78168fe6e0f7)

## 📊 Results and Analysis


### Test Case - **Comparing words across different categories:** 

  ![Cosine Similarity Matrix](https://github.com/user-attachments/assets/0b3c8cc6-3396-4315-99b5-d46edd15b4d9)  

- **Bar Graph Representation:**  
  ![Bar Graph](https://github.com/user-attachments/assets/9eb99daf-f311-498a-bcfa-8d2d61ec2c33)  



## **Team Members**

-  Muhammad Ahsan Ijaz
-  Aman Basha Patel
-  Saqib Attar

---



