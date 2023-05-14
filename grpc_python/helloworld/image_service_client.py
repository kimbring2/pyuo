# Copyright 2015 gRPC authors.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

"""The Python implementation of the GRPC helloworld.Greeter client."""

from __future__ import print_function

import grpc

import helloworld_pb2
import helloworld_pb2_grpc

import io

from PIL import Image
import time
import numpy as np
import cv2


def run():
  channel = grpc.insecure_channel('localhost:50052')
  stub = helloworld_pb2_grpc.ImageServiceStub(channel)

  for i in range(0, 10000):
    print("i: ", i)

    response = stub.ProcessImage(helloworld_pb2.HelloRequest(name='you'))
    image_data = response.data
    image_data = io.BytesIO(image_data).read()
    #image = Image.open(io.BytesIO(image_data))
    #image.show()
    image = Image.frombuffer("RGBA", (640,480), image_data, "raw", "BGRA", 0, 1)
    obs = np.ndarray(shape=(480,640,4), dtype=np.uint8, buffer=image_data)
    #a_copy = a.copy()
    #(a_copy[:,:,0], a_copy[:,:,2]) = (a_copy[:,:,2], a_copy[:,:,0])
    #img.convert('RGBA').tobytes('raw', 'BGRA', 0, 1)

    cv2.imshow('obs', obs)
    cv2.waitKey(1)

    #image.show()
    #numpy_array = np.frombuffer(image_data, np.uint8)
    #print("numpy_array: ", numpy_array)
    #time.sleep(0.5)
    #break


if __name__ == '__main__':
  run()
