# **Semantic Analysis Of Textual Data**

## **Project Overview**

This project presents a structured approach to **semantic similarity analysis of textual data**, addressing the need for **effective processing and extraction of insights from unstructured content**. The framework is designed with a **modular architecture**, ensuring adaptability across different domains while maintaining **computational efficiency** through **parallel processing and chunking techniques**.

The system delivers **accurate and meaningful results** while optimizing performance, making it a scalable and efficient solution for various applications, including **document clustering, content recommendation, anomaly detection, and information retrieval**.

## **Project Workflow**

### **Flow Diagram**

The following diagram represents the complete project workflow:

![Project workflow diagram showing the data processing pipeline from raw documents to similarity matrix](https://github.com/user-attachments/assets/102154c6-b450-4dfc-8d37-3c366956ff0a)<br>
*Flowchart depicting the three-step process of Semantic Analysis Of Textual Data.*

### 1. **Document Cleaning and Extraction**

- **Input**: Two Raw documents (e.g., .docx, PDF, .md, csv)
- **Process**:
  - Text cleaning: Remove noise, special characters, and unnecessary formatting
  - Filtering: Extract relevant information for analysis
- **Output**: Two structured JSON files representing the cleaned and filtered data

### 2. **Embedding Generation**

- **Input**: JSON files created in the previous step
- **Process**:
  - Utilizing the JSON files
  - Generate embeddings for the text using OpenAI's embedding models
- **Output**: CSV files with two columns:
  - **Text**: The original extracted data
  - **Embedding**: Numerical vectors representing semantic meaning

### 3. **Cosine Similarity Calculation**

- **Input**: CSV files with embeddings
- **Process**:
  - Calculate the cosine similarity between embeddings from two CSV input files to measure the semantic similarity between text data points
- **Output**: A CSV file representing a similarity matrix

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

## **Setup Instructions for Running the Project**

### **Prerequisites**

- .NET SDK
- Ensure that the API key is set as an environment variable with the name `OPENAI_API_KEY`
  .Use the following command to setup API key through Command Prompt:
   ```sh
    setx OPENAI_API_KEY "your_api_key_here"
   ``` 
  
  
### Steps to Run the Program  

1. **Clone the repository**
   ```sh
   git clone https://github.com/PatelAman09/semantic-analysis.git
   ```    
2. **Build the project** located at: `semantic-analysis\Semantic_Analysis\Semantic_Analysis`  
   ```sh
   dotnet build
   ```
3. Place the two documents you want to compare in: `semantic-analysis\Semantic_Analysis\Semantic_Analysis\RawData`<br>
   Our program supports the following file formats:
   - Plain text: `.txt`, `.md`
   - Structured documents: `.json`, `.xml`, `.html`
   - Rich text files: `.pdf`, `.docx`


4. Ensure the following folders are empty before running the program:
   - `ExtractedData`
   - `EmbeddingOutput`
   - `CSVOutput`
5. Navigate to `bin/Debug/(Your .NET version)` and execute **Semantic_Analysis.exe**
6. After running the console application, following prompt will appear:

   ![Embedding type selection prompt showing options for word-by-word/phrase-by-phrase or entire document](https://github.com/user-attachments/assets/3bf89047-4d42-432e-b042-3830c69a6272)<br>
   *Console interface showing the two embedding options available to user for processing the documents.*<br>
   - Option 1: Generate embeddings for each word or phrase in the document (suitable for word-by-word or phrase-by-phrase comparison).<br>
   - Option 2: Generate a single embedding for the entire document (suitable for document-level comparison).

8. The final output file, containing the cosine similarity calculation will be located at:<br>`semantic-analysis\Semantic_Analysis\Semantic_Analysis\CSVOutput`
   - Sample Output from **CSVOutput** folder:
   - ![Screenshot of CSV output showing the cosine similarity calculation results in tabular format](https://github.com/user-attachments/assets/f402eba7-ffa6-437a-9f14-78168fe6e0f7)<br>
*Tabular output displaying texts and their corresponding similarity scores in output folder.*

## **Sample Result**

### **Test Case - Comparing Words Across Different Categories**

![Cosine Similarity Matrix showing semantic relationships between different words](https://github.com/user-attachments/assets/0b3c8cc6-3396-4315-99b5-d46edd15b4d9)<br>
*Table of semantic relationships between reference words and five distinct categories with similarity values.*

This test case was conducted using the `text-embedding-3-small` model. The matrix represents semantic similarity scores between reference words and five categories: Technology, Biology, Restaurant, Mathematics, and Arts & Music. Higher values indicate stronger semantic associations. Notable observations include:

- "Robotics" shows strongest association with Technology (0.5886) and weakest with Arts & Music (0.3800)
- "Algebra" demonstrates clear alignment with Mathematics (0.6993)
- "Photosynthesis" exhibits significant association with Biology (0.4883)

### **Visual Representation**

![Bar Graph showing comparative cosine similarity values between different terms](https://github.com/user-attachments/assets/9eb99daf-f311-498a-bcfa-8d2d61ec2c33)<br>
*Bar chart visualizing the distribution of similarity scores across categories, highlighting domain-specific semantic relationships.*

The bar graph visualizes the distribution of similarity scores across categories, making patterns more discernible. The visual representation effectively highlights how reference words naturally cluster within their expected domains, validating the semantic analysis model's accuracy in identifying contextual similarities between terms and their categorical associations.
