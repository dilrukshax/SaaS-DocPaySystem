# Load Testing Script for SaaS DocPay System
# Run with: python load_test.py

import requests
import time
import concurrent.futures
import statistics

BASE_URL = "http://localhost:5000"

def test_health_endpoint():
    """Test the health endpoint"""
    try:
        response = requests.get(f"{BASE_URL}/health", timeout=5)
        return response.status_code == 200
    except:
        return False

def test_service_health(service):
    """Test individual service health"""
    try:
        response = requests.get(f"{BASE_URL}/health/{service}", timeout=5)
        return response.status_code == 200
    except:
        return False

def load_test_health(num_requests=100, num_workers=10):
    """Perform load testing on health endpoints"""
    print(f"Starting load test: {num_requests} requests with {num_workers} workers")
    
    start_time = time.time()
    success_count = 0
    response_times = []
    
    def make_request():
        request_start = time.time()
        success = test_health_endpoint()
        request_time = time.time() - request_start
        return success, request_time
    
    with concurrent.futures.ThreadPoolExecutor(max_workers=num_workers) as executor:
        futures = [executor.submit(make_request) for _ in range(num_requests)]
        
        for future in concurrent.futures.as_completed(futures):
            success, response_time = future.result()
            if success:
                success_count += 1
            response_times.append(response_time)
    
    total_time = time.time() - start_time
    
    print(f"\nLoad Test Results:")
    print(f"Total Requests: {num_requests}")
    print(f"Successful Requests: {success_count}")
    print(f"Failed Requests: {num_requests - success_count}")
    print(f"Success Rate: {(success_count/num_requests)*100:.2f}%")
    print(f"Total Time: {total_time:.2f} seconds")
    print(f"Requests/Second: {num_requests/total_time:.2f}")
    print(f"Average Response Time: {statistics.mean(response_times):.3f} seconds")
    print(f"Min Response Time: {min(response_times):.3f} seconds")
    print(f"Max Response Time: {max(response_times):.3f} seconds")

if __name__ == "__main__":
    # Test individual endpoints first
    print("Testing individual endpoints...")
    services = ["users", "payments", "notifications", "workflows"]
    
    for service in services:
        if test_service_health(service):
            print(f"✅ {service.capitalize()} Service: Healthy")
        else:
            print(f"❌ {service.capitalize()} Service: Failed")
    
    print(f"\n{'='*50}")
    
    # Run load test
    load_test_health(num_requests=50, num_workers=5)
