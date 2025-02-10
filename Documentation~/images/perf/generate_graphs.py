import xml.etree.ElementTree as ET
import json
import re
import matplotlib.pyplot as plt
import numpy as np
import os
from collections import defaultdict
from functools import reduce
from aquarel import load_theme

#theme = load_theme("scientific")
#theme.apply()

# Helper function to extract balanced JSON from a string after the marker
def extract_balanced_json(text):
    start_index = text.find('{')
    if start_index == -1:
        return None  # No JSON found

    brace_count = 0
    json_str = ""
    
    for i in range(start_index, len(text)):
        char = text[i]
        if char == '{':
            brace_count += 1
        elif char == '}':
            brace_count -= 1
        
        json_str += char
        
        if brace_count == 0:
            break
    
    if brace_count != 0:
        return None
    
    return json_str

# Function to extract performance test results from <test-suite> tags
def extract_performance_test_results(xml_file):
    tree = ET.parse(xml_file)
    root = tree.getroot()

    # Regex to match the start of performance test result JSON (non-greedy)
    pattern = re.compile(r'##performancetestresult2:({.*?})[\n|<]', re.DOTALL)

    results = []
    
    for output in root.findall(".//output"):
        output_text = output.text if output.text else ""
        
        # Search for JSON content inside the <test-suite> tag text
        for match in pattern.finditer(output_text):
            json_str = extract_balanced_json(match.group(1))
            if json_str:
                try:
                    result = json.loads(json_str)
                    results.append(result)
                except json.JSONDecodeError:
                    print("Error decoding JSON: ", json_str)
            else:
                print("Could not extract balanced JSON.")
    
    return results

# Function to convert Median values based on the Unit field
def convert_to_milliseconds(median, unit):
    if unit == 0:  # Nanoseconds to milliseconds
        return median * 1e-6
    elif unit == 1:  # Microseconds to milliseconds
        return median * 1e-3
    elif unit == 2:  # Milliseconds (no conversion needed)
        return median
    else:
        return median  # Fallback if the unit is not recognized

# Function to sum the converted Median fields from the SampleGroups array
def sum_median_sample_groups(result):
    sample_groups = result.get('SampleGroups', [])
    total_median_ms = 0
    for group in sample_groups:
        median = group.get('Median', 0)
        unit = group.get('Unit', 2)  # Default to milliseconds if the Unit is not provided
        total_median_ms += convert_to_milliseconds(median, unit)
    return total_median_ms

def recursive_get(d, keys):
    return reduce(lambda c, k: c.get(k, {}), keys, d)

def recursive_set(d, value, keys):
    if len(keys) == 1:
        d[keys[0]] = value
    else:
        d = d.setdefault(keys[0], {})
        recursive_set(d, value, keys[1:])
        
# Function to create a single comparison bar chart
def get_structured_results(results):
    # Regex pattern to extract category and test name from "Name"
    name_pattern = re.compile(r'^Tests\.(.*)\.(.*)_(.*)\((.*)\)$')

    # Dictionary to store test results
    test_results = {}
    
    for result in results:
        name = result.get('Name')

        if name:
            match = name_pattern.match(name)
            if match:
                category = match.group(1)
                test_prefix = match.group(2)
                test_suffix = match.group(3)
                count = match.group(4)

                keys = [test_prefix, test_suffix, category, count]
                existing = recursive_get(test_results, keys)
                if existing:
                    continue

                # Sum the median values for the test result (converted to milliseconds)
                performance_value = sum_median_sample_groups(result)
                recursive_set(test_results, performance_value, keys)
        
    print(json.dumps(test_results, indent=4))
    
    return test_results

def plot_structured_results(data, output_dir='svg_plots'):
    # Create output directory if it doesn't exist
    os.makedirs(output_dir, exist_ok=True)

    for root_key, root_value in data.items():
        for second_key, second_value in root_value.items():
            plt.figure(figsize=(10, 5))
            plt.title(f'Performance: {root_key} - {second_key}', fontsize=16)
            
            bar_labels = []
            bar_data = []
            x_labels = None
            max_value = 0
            
            # Collect valid data (skip categories with all 0 values)
            for third_key, third_value in second_value.items():
                values = list(third_value.values())
                if any(val != 0 for val in values):  # Only include if not all values are 0
                    if x_labels is None:
                        x_labels = list(third_value.keys())  # 4th level keys for x-axis
                    bar_labels.append(third_key)  # 3rd level keys (category labels)
                    bar_data.append(values)  # Corresponding y-values
                    max_value = max(max_value, max(values))
            
            # Make space for labels and legend
            plt.ylim(top=max_value*1.2)
            
            if bar_labels:
                # Calculate bar positions dynamically
                bar_width = 0.2
                bar_space = 0.1
                
                x = np.arange(len(x_labels))
                num_bars = len(bar_data)
                half_width = bar_width * num_bars * 0.333
                
                # Ensures proper centering in case of 1 bar
                if num_bars == 1:
                    half_width = 0
                    
                offsets = np.linspace(-half_width, half_width, num_bars)
                # Plot each valid category
                for i, (label, y_values) in enumerate(zip(bar_labels, bar_data)):
                    bars = plt.bar(x + offsets[i], y_values, bar_width, label=label)
                    # Add text labels on top of each bar
                    for bar in bars:
                        height = bar.get_height()
                        if height != 0:
                            plt.annotate(f'{height:.2f}',
                                         xy=(bar.get_x() + bar.get_width() / 2, height),
                                         xytext=(0, 3),  # Offset above the bar
                                         textcoords="offset points",
                                         ha='center', va='bottom')
                # Set x-axis labels
                plt.xticks(x, x_labels)
                plt.xlabel('Test Size')
                plt.ylabel('Execution Time (ms)')
                plt.legend(title="Test Suite")
            
            plt.tight_layout()
            
            # Save as SVG instead of showing the plot
            filename = f"{root_key}_{second_key}.svg".replace(" ", "_").lower()
            filepath = os.path.join(output_dir, filename)
            plt.savefig(filepath, format='svg')
            plt.close()  # Close the figure to free up memory

    print(f"SVG files have been saved in the '{output_dir}' directory.")
        
# Main function
def main(xml_file):
    results = extract_performance_test_results(xml_file)
    
    if results:
        structured_results = get_structured_results(results)
        plot_structured_results(structured_results)
    else:
        print("No performance test results found.")

# Example usage:
xml_file = 'TestResults.xml'
main(xml_file)
